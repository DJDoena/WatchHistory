namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AbstractionLayer.IOServices;
    using MediaInfoHelper;
    using ToolBox.Extensions;

    internal sealed class DataManager : IDataManager
    {
        private readonly IIOServices _ioServices;

        private readonly IFileObserver _fileObserver;

        private readonly object _filesLock;

        private readonly IFilesSerializer _filesSerializer;

        private readonly string _dataFile;

        private readonly string _settingsFile;

        private static readonly DateTime _turnOfTheCentury;

        private IEnumerable<string> _rootFolders;

        private IEnumerable<string> _fileExtensions;

        private IEnumerable<string> _users;

        private bool _isSynchronizing;

        private Dictionary<string, FileEntry> Files { get; set; }

        private bool IsSuspended { get; set; }

#pragma warning disable IDE1006 // Naming Styles
        private event EventHandler _filesChanged;
#pragma warning restore IDE1006 // Naming Styles

        static DataManager()
        {
            _turnOfTheCentury = new DateTime(1999, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        }

        public DataManager(string settingsFile, string dataFile, IIOServices ioServices)
        {
            _ioServices = ioServices;

            _filesLock = new object();

            _settingsFile = settingsFile;

            _dataFile = dataFile;

            _fileObserver = new FileObserver(ioServices);

            _filesSerializer = new FilesSerializer(ioServices);

            _filesSerializer.CreateBackup(settingsFile);

            this.LoadSettings();

            _filesSerializer.CreateBackup(dataFile);

            this.LoadData();
        }

        #region IDataManager

        public IEnumerable<string> RootFolders
        {
            get => _rootFolders.Select(folder => folder);
            set
            {
                value = new HashSet<string>(value);

                if (this.IsSuspended == false)
                {
                    _fileObserver.Observe(value, _fileExtensions);
                }

                _rootFolders = value.ToList();

                this.SyncData();
            }
        }

        public IEnumerable<string> FileExtensions
        {
            get => _fileExtensions.Select(ext => ext);
            set
            {
                value = value.Select(ext => ext.ReplaceInvalidFileNameChars('_'));

                value = new HashSet<string>(value);

                if (this.IsSuspended == false)
                {
                    _fileObserver.Observe(_rootFolders, value);
                }

                _fileExtensions = value.ToList();

                this.SyncData();
            }
        }

        public IEnumerable<string> Users
        {
            get => _users.Select(user => user);
            set
            {
                value = new HashSet<string>(value);

                _users = value.ToList();
            }
        }

        public bool IsSynchronizing
        {
            get => _isSynchronizing;
            private set
            {
                _isSynchronizing = value;

                IsSynchronizingChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler IsSynchronizingChanged;

        public event EventHandler FilesChanged
        {
            add
            {
                if (_filesChanged == null)
                {
                    _fileObserver.Created += this.OnFileCreated;
                    _fileObserver.Deleted += this.OnFileDeleted;
                }

                _filesChanged += value;
            }
            remove
            {
                _filesChanged -= value;

                if (_filesChanged == null)
                {
                    _fileObserver.Created -= this.OnFileCreated;
                    _fileObserver.Deleted -= this.OnFileDeleted;
                }
            }
        }

        public IEnumerable<FileEntry> GetFiles()
        {
            lock (_filesLock)
            {
                return this.Files.Values;
            }
        }

        public void AddWatched(FileEntry entry, string userName) => this.AddWatched(entry, userName, DateTime.UtcNow);

        public void AddWatched(FileEntry entry, string userName, DateTime watchedOn)
        {
            lock (_filesLock)
            {
                var user = entry.TryGetUser(userName) ?? this.AddUser(entry, userName);

                this.AddWatched(user, watchedOn);
            }

            this.RaiseFilesChanged();
        }

        public void AddIgnore(FileEntry entry, string userName)
        {
            lock (_filesLock)
            {
                var user = entry.TryGetUser(userName) ?? this.AddUser(entry, userName);

                user.Ignore = true;
            }

            this.RaiseFilesChanged();
        }

        public void UndoIgnore(FileEntry entry, string userName)
        {
            var user = entry.TryGetUser(userName);

            if (user != null)
            {
                user.Ignore = false;

                this.RaiseFilesChanged();
            }
        }

        public DateTime GetLastWatched(FileEntry entry, string userName)
        {
            var user = entry.TryGetUser(userName);

            if (user?.Watches?.HasItems() == true)
            {
                var lastWatched = user.Watches.Max(watch => watch.Value).ToLocalTime();

                return lastWatched;
            }
            else
            {
                return new DateTime(0, DateTimeKind.Local);
            }
        }

        public DateTime GetCreationTime(FileEntry entry)
        {
            var fi = _ioServices.GetFileInfo(entry.FullName);

            var creationTime = new DateTime(0, DateTimeKind.Utc);

            if (fi.Exists)
            {
                creationTime = fi.CreationTimeUtc;
            }

            return creationTime;
        }

        public MediaFileData DetermineVideoLength(FileEntry entry)
        {
            MediaFileData mediaFileData;
            try
            {
                mediaFileData = new MediaFileData(entry.FullName, entry.CreationTime, entry.VideoLength);

                (new VideoReader(mediaFileData, false)).DetermineLength();
            }
            catch
            {
                mediaFileData = new MediaFileData(entry.FullName, entry.CreationTime, 0);
            }

            return mediaFileData;
        }

        public void SaveSettingsFile()
        {
            var defaultValues = new DefaultValues()
            {
                Users = _users.ToArray(),
                RootFolders = _rootFolders.ToArray(),
                FileExtensions = _fileExtensions.ToArray(),
            };

            _filesSerializer.SaveSettings(_settingsFile, defaultValues);
        }

        public void SaveDataFile()
        {
            lock (_filesLock)
            {
                var entries = this.Files.Values.ToList();

                entries.Sort((left, right) =>
                {
                    var compare = 0;
                    if (left.TitleSpecified && !right.TitleSpecified)
                    {
                        compare = -1;
                    }
                    else if (!left.TitleSpecified && right.TitleSpecified)
                    {
                        compare = 1;
                    }
                    else if (left.TitleSpecified && right.TitleSpecified)
                    {
                        compare = left.Title.CompareTo(right.Title);
                    }

                    if (compare == 0)
                    {
                        compare = this.GetCompareName(left).CompareTo(this.GetCompareName(right));
                    }

                    if (compare == 0)
                    {
                        compare = left.FullName.CompareTo(right.FullName);
                    }

                    return compare;
                });

                _filesSerializer.SaveData(_dataFile, entries);
            }
        }

        private string GetCompareName(FileEntry left)
        {
            var name = left.FullName;

            foreach (var rootFolder in _rootFolders)
            {
                name = name.Replace(rootFolder, string.Empty);
            }

            name = name.TrimStart('\\');

            return name;
        }

        public void Suspend()
        {
            this.IsSuspended = true;

            _fileObserver.Suspend();
        }

        public void Resume()
        {
            this.IsSuspended = false;

            _fileObserver.Observe(_rootFolders, _fileExtensions);

            this.SyncData();
        }

        public FileEntry TryCreateEntry(FileEntry newFileEntry)
        {
            var result = newFileEntry;

            lock (_filesLock)
            {
                var key = newFileEntry.Key;

                if (this.Files.TryGetValue(key, out var existingFileEntry))
                {
                    this.MergeEntry(existingFileEntry, newFileEntry);

                    result = existingFileEntry;
                }
                else
                {
                    this.Files.Add(key, newFileEntry);
                }
            }

            this.RaiseFilesChanged();

            return result;
        }

        #endregion

        private void LoadSettings()
        {
            var defaultValues = _filesSerializer.LoadSettings(_settingsFile);

            _users = defaultValues.Users;

            _rootFolders = defaultValues.RootFolders;

            _fileExtensions = defaultValues.FileExtensions;
        }

        private void LoadData()
        {
            var entries = _filesSerializer.LoadData(_dataFile);

            lock (_filesLock)
            {
                this.Files = new Dictionary<string, FileEntry>(entries.Count());

                entries.ForEach(entry =>
                {
                    if (!this.Files.TryGetValue(entry.Key, out var existing))
                    {
                        this.Files.Add(entry.Key, entry);
                    }
                    else
                    {
                        this.MergeEntry(existing, entry);
                    }
                });
            }

            this.RaiseFilesChanged();
        }

        private User AddUser(FileEntry entry, string userName)
        {
            var users = entry.Users?.ToList() ?? new List<User>(1);

            var user = new User()
            {
                UserName = userName
            };

            users.Add(user);

            entry.Users = users.ToArray();

            return user;
        }

        private void AddWatched(User user, DateTime watchedOn)
        {
            var watches = user.Watches?.ToList() ?? new List<Watch>(1);

            watches.Add(new Watch()
            {
                Value = watchedOn
            });

            user.Watches = watches.ToArray();
        }

        private void SyncData()
        {
            if (this.IsSuspended == false)
            {
                this.GetActualFiles();
            }
        }

        private bool HasValidEvents(FileEntry entry) => entry.Users?.HasItemsWhere(HasEvents) == true;

        private static bool HasEvents(User user) => user.Watches?.HasItemsWhere(w => w.SourceSpecified == false && w.Value > _turnOfTheCentury) == true;

        private bool IsProtected(FileEntry entry) => entry.FullName.EndsWith(Constants.DvdProfilerFileExtension) && entry.VideoLengthSpecified;

        private void GetActualFiles()
        {
            this.IsSynchronizing = true;

            var tasks = _rootFolders.Select(this.GetActualFiles).SelectMany(task => task);

            var readyTask = Task.WhenAll(tasks);

            readyTask.ContinueWith(this.ProcessActualFiles);
        }

        private IEnumerable<Task<IEnumerable<string>>> GetActualFiles(string folder) => _fileExtensions.Select(ext => this.GetActualFiles(folder, ext));

        private Task<IEnumerable<string>> GetActualFiles(string folder, string fileExtension) => Task.Run(() => this.GetFiles(folder, fileExtension));

        private void ProcessActualFiles(Task<IEnumerable<string>[]> task)
        {
            var actualFiles = task.Result.SelectMany(file => file).ToList();

            lock (_filesLock)
            {
                actualFiles.ForEach(this.AddActualFile);

                this.RemoveObsoletesFiles(actualFiles);

                this.SaveDataFile();
            }

            this.IsSynchronizing = false;

            this.RaiseFilesChanged();
        }

        private IEnumerable<string> GetFiles(string rootFolder, string fileExtension)
            => _ioServices.Folder.Exists(rootFolder)
                ? (_ioServices.Folder.GetFileNames(rootFolder, "*." + fileExtension, System.IO.SearchOption.AllDirectories))
                : (Enumerable.Empty<string>());

        private void AddActualFile(string actualFile)
        {
            FileEntry entry;
            lock (_filesLock)
            {
                var key = FileEntry.GetKey(actualFile);

                if (this.Files.TryGetValue(key, out entry) == false)
                {
                    entry = new FileEntry()
                    {
                        FullName = actualFile,
                        FileExists = true,
                    };

                    entry.CreationTime = entry.GetCreationTime(this);

                    this.Files.Add(key, entry);
                }
            }

            if (entry.TitleSpecified == false)
            {
                (new VideoInfoAdder(_ioServices, entry)).Add();
            }

            if (actualFile.EndsWith(Constants.DvdProfilerFileExtension))
            {
                (new DvdWatchesProcessor(_ioServices)).Update(entry);
            }
            else if (actualFile.EndsWith(Constants.YoutubeFileExtension))
            {
                (new YoutubeWatchesProcessor(_ioServices)).Update(entry);
            }
        }

        private void RemoveObsoletesFiles(IEnumerable<string> actualFiles)
        {
            lock (_filesLock)
            {
                var files = this.Files.ToList();

                var actualFileKeys = actualFiles.Select(file => FileEntry.GetKey(file)).ToList();

                files.ForEach(file => this.TryRemoveFile(actualFileKeys, file));
            }
        }

        private void TryRemoveFile(List<string> actualFileKeys, KeyValuePair<string, FileEntry> file)
        {
            lock (_filesLock)
            {
                if ((actualFileKeys.Contains(file.Key) == false) && (this.HasValidEvents(file.Value) == false) && (this.IsProtected(file.Value) == false))
                {
                    this.Files.Remove(file.Key);
                }
                else
                {
                    file.Value.FileExists = actualFileKeys.Contains(file.Key);
                }
            }
        }

        private void OnFileDeleted(object sender, System.IO.FileSystemEventArgs e)
        {
            lock (_filesLock)
            {
                var key = FileEntry.GetKey(e.FullPath);

                if (this.Files.TryGetValue(key, out var entry) && (this.HasValidEvents(entry) == false) && (this.IsProtected(entry) == false))
                {
                    this.Files.Remove(key);
                }
                else if (entry != null)
                {
                    entry.FileExists = _ioServices.File.Exists(entry.FullName);
                }
            }

            this.RaiseFilesChanged();
        }

        private void OnFileCreated(object sender, System.IO.FileSystemEventArgs e)
        {
            var entry = new FileEntry()
            {
                FullName = e.FullPath,
            };

            lock (_filesLock)
            {
                var key = entry.Key;

                if (this.Files.ContainsKey(key) == false)
                {
                    this.Files.Add(key, entry);
                }
            }

            this.RaiseFilesChanged();
        }

        private void RaiseFilesChanged() => _filesChanged?.Invoke(this, EventArgs.Empty);

        private void MergeEntry(FileEntry existingEntry, FileEntry newEntry)
        {
            existingEntry.Title = newEntry.Title;

            if (newEntry.VideoLength > 0)
            {
                existingEntry.VideoLength = newEntry.VideoLength;
            }

            if (!string.IsNullOrWhiteSpace(newEntry.Note))
            {
                existingEntry.Note = newEntry.Note;
            }

            MergeUsers(existingEntry, newEntry);
        }

        private static void MergeUsers(FileEntry existingEntry, FileEntry newEntry)
        {
            if (newEntry.Users == null)
            {
                return;
            }
            else if (existingEntry.Users == null)
            {
                existingEntry.Users = newEntry.Users;

                return;
            }

            foreach (var newUser in newEntry.Users)
            {
                MergeUser(existingEntry, newUser);
            }
        }

        private static void MergeUser(FileEntry existingEntry, User newUser)
        {
            var existingUser = existingEntry.TryGetUser(newUser.UserName);

            if (existingUser == null)
            {
                existingEntry.Users = newUser.Enumerate().Union(existingEntry.Users).ToArray();

                return;
            }

            MergeWatches(newUser, existingUser);
        }

        private static void MergeWatches(User newUser, User existingUser)
        {
            if (newUser.Watches == null)
            {
                return;
            }
            else if (existingUser.Watches == null)
            {
                existingUser.Watches = newUser.Watches;

                return;
            }

            var watches = new List<Watch>(existingUser.Watches);

            foreach (var newWatch in newUser.Watches)
            {
                if (!watches.Any(existingWatch => existingWatch.Source == newWatch.Source && existingWatch.Value == newWatch.Value))
                {
                    watches.Add(newWatch);
                }
            }
        }
    }
}