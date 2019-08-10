namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AbstractionLayer.IOServices;
    using ToolBox.Extensions;
    using WatchHistory.Implementations;

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

        private event EventHandler _FilesChanged;

        static DataManager()
        {
            _turnOfTheCentury = new DateTime(1999, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        }

        public DataManager(string settingsFile
            , string dataFile
            , IIOServices ioServices)
        {
            _ioServices = ioServices;

            _filesLock = new object();

            _fileObserver = new FileObserver(ioServices);

            _settingsFile = settingsFile;

            LoadSettings();

            _filesSerializer = new FilesSerializer(ioServices);

            _filesSerializer.CreateBackup(dataFile);

            _dataFile = dataFile;

            LoadData();
        }

        #region IDataManager

        public IEnumerable<string> RootFolders
        {
            get => _rootFolders.Select(folder => folder);
            set
            {
                value = new HashSet<string>(value);

                if (IsSuspended == false)
                {
                    _fileObserver.Observe(value, _fileExtensions);
                }

                _rootFolders = value.ToList();

                SyncData();
            }
        }

        public IEnumerable<string> FileExtensions
        {
            get => _fileExtensions.Select(ext => ext);
            set
            {
                value = value.Select(ext => ext.ReplaceInvalidFileNameChars('_'));

                value = new HashSet<string>(value);

                if (IsSuspended == false)
                {
                    _fileObserver.Observe(_rootFolders, value);
                }

                _fileExtensions = value.ToList();

                SyncData();
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
                if (_FilesChanged == null)
                {
                    _fileObserver.Created += OnFileCreated;
                    _fileObserver.Deleted += OnFileDeleted;
                }

                _FilesChanged += value;
            }
            remove
            {
                _FilesChanged -= value;

                if (_FilesChanged == null)
                {
                    _fileObserver.Created -= OnFileCreated;
                    _fileObserver.Deleted -= OnFileDeleted;
                }
            }
        }

        public IEnumerable<FileEntry> GetFiles()
        {
            lock (_filesLock)
            {
                return Files.Values;
            }
        }

        public void AddWatched(FileEntry entry
           , string userName)
            => AddWatched(entry, userName, DateTime.UtcNow);

        public void AddWatched(FileEntry entry
            , string userName
            , DateTime watchedOn)
        {
            lock (_filesLock)
            {
                var user = TryGetUser(entry, userName);

                user = user ?? AddUser(entry, userName);

                AddWatched(user, watchedOn);
            }

            RaiseFilesChanged();
        }

        public void AddIgnore(FileEntry entry
            , string userName)
        {
            lock (_filesLock)
            {
                var user = TryGetUser(entry, userName) ?? AddUser(entry, userName);

                user.Ignore = true;
            }

            RaiseFilesChanged();
        }

        public void UndoIgnore(FileEntry entry
            , string userName)
        {
            var user = TryGetUser(entry, userName);

            if (user != null)
            {
                user.Ignore = false;

                RaiseFilesChanged();
            }
        }

        public DateTime GetLastWatched(FileEntry entry
            , string userName)
        {
            var user = TryGetUser(entry, userName);

            var lastWatched = new DateTime(0, DateTimeKind.Local);

            if (user?.Watches?.HasItems() == true)
            {
                lastWatched = user.Watches.Max(watch => watch.Value).ToLocalTime();
            }

            return lastWatched;
        }

        public DateTime GetCreationTime(FileEntry fileEntry)
        {
            var fi = _ioServices.GetFileInfo(fileEntry.FullName);

            var creationTime = new DateTime(0);

            if (fi.Exists)
            {
                creationTime = fi.CreationTime;
            }

            return creationTime;
        }

        public UInt32 GetVideoLength(FileEntry fileEntry)
        {
            try
            {
                var videoReader = new VideoReader(_ioServices, fileEntry);

                var videoLength = videoReader.GetLength();

                return videoLength;
            }
            catch
            {
                return 0;
            }
        }
        public void SaveSettingsFile()
        {
            var defaultValues = new DefaultValues()
            {
                Users = _users.ToArray(),
                RootFolders = _rootFolders.ToArray(),
                FileExtensions = _fileExtensions.ToArray()
            };

            var settings = new Settings()
            {
                DefaultValues = defaultValues
            };

            SerializerHelper.Serialize(_ioServices, _settingsFile, settings);
        }

        public void SaveDataFile()
        {
            lock (_filesLock)
            {
                var entries = Files.Values.ToList();

                entries.Sort((left, right) => left.FullName.CompareTo(right.FullName));

                var files = new Files()
                {
                    Entries = entries.ToArray()
                };

                _filesSerializer.SaveFile(_dataFile, files);
            }
        }

        public void Suspend()
        {
            IsSuspended = true;

            _fileObserver.Suspend();
        }

        public void Resume()
        {
            IsSuspended = false;

            _fileObserver.Observe(_rootFolders, _fileExtensions);

            SyncData();
        }

        public FileEntry TryCreateEntry(FileEntry newFileEntry)
        {
            var result = newFileEntry;

            lock (_filesLock)
            {
                var key = newFileEntry.Key;

                if (Files.TryGetValue(key, out var existingFileEntry))
                {
                    MergeEntry(existingFileEntry, newFileEntry);

                    result = existingFileEntry;
                }
                else
                {
                    Files.Add(key, newFileEntry);
                }
            }

            RaiseFilesChanged();

            return result;
        }

        #endregion

        private void LoadSettings()
        {
            Settings settings;
            try
            {
                settings = SerializerHelper.Deserialize<Settings>(_ioServices, _settingsFile);
            }
            catch
            {
                settings = new Settings()
                {
                    DefaultValues = new DefaultValues()
                };
            }

            _users = settings?.DefaultValues?.Users ?? Enumerable.Empty<string>();

            _rootFolders = settings?.DefaultValues?.RootFolders ?? Enumerable.Empty<string>();

            _fileExtensions = settings?.DefaultValues?.FileExtensions ?? Enumerable.Empty<string>();
        }

        private void LoadData()
        {
            var files = _filesSerializer.LoadData(_dataFile);

            lock (_filesLock)
            {
                Files = new Dictionary<string, FileEntry>(files?.Entries?.Length ?? 0);

                var entries = files?.Entries ?? Enumerable.Empty<FileEntry>();

                entries.ForEach(entry => Files[entry.Key] = entry);
            }
        }

        private User AddUser(FileEntry entry
            , string userName)
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

        private void AddWatched(User user
            , DateTime watchedOn)
        {
            var watches = user.Watches?.ToList() ?? new List<Watch>(1);

            watches.Add(new Watch()
            {
                Value = watchedOn
            });

            user.Watches = watches.ToArray();
        }

        private static User TryGetUser(FileEntry entry
            , string userName)
            => entry.Users?.Where(user => IsUser(user, userName)).FirstOrDefault();

        private static bool IsUser(User user
            , string userName)
            => user.UserName == userName;

        private void SyncData()
        {
            if (IsSuspended == false)
            {
                GetActualFiles();
            }
        }

        private bool HasValidEvents(FileEntry entry)
            => entry.Users?.HasItemsWhere(HasEvents) == true;

        private static bool HasEvents(User user)
            => user.Watches?.HasItemsWhere(w => w.SourceSpecified == false && w.Value > _turnOfTheCentury) == true;

        private bool IsProtected(FileEntry entry)
            => entry.FullName.EndsWith(Constants.DvdProfilerFileExtension) && entry.VideoLengthSpecified;

        private void GetActualFiles()
        {
            IsSynchronizing = true;

            var tasks = _rootFolders.Select(GetActualFiles).SelectMany(task => task);

            var readyTask = Task.WhenAll(tasks);

            readyTask.ContinueWith(ProcessActualFiles);
        }

        private IEnumerable<Task<IEnumerable<string>>> GetActualFiles(string folder)
            => _fileExtensions.Select(ext => GetActualFiles(folder, ext));

        private Task<IEnumerable<string>> GetActualFiles(string folder, string fileExtension)
            => Task.Run(() => GetFiles(folder, fileExtension));

        private void ProcessActualFiles(Task<IEnumerable<string>[]> task)
        {
            var actualFiles = task.Result.SelectMany(file => file).ToList();

            lock (_filesLock)
            {
                actualFiles.ForEach(AddActualFile);

                RemoveObsoletesFiles(actualFiles);

                SaveDataFile();
            }

            IsSynchronizing = false;

            RaiseFilesChanged();
        }

        private IEnumerable<string> GetFiles(string rootFolder
            , string fileExtension)
            => _ioServices.Folder.Exists(rootFolder)
                ? (_ioServices.Folder.GetFiles(rootFolder, "*." + fileExtension, System.IO.SearchOption.AllDirectories))
                : (Enumerable.Empty<string>());

        private void AddActualFile(string actualFile)
        {
            FileEntry entry;
            lock (_filesLock)
            {
                var key = FileEntry.GetKey(actualFile);

                if (Files.TryGetValue(key, out entry) == false)
                {
                    entry = new FileEntry()
                    {
                        FullName = actualFile,
                    };

                    entry.CreationTime = entry.GetCreationTime(this);

                    Files.Add(key, entry);
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
                var files = Files.ToList();

                var fileKeys = actualFiles.Select(file => file.ToLower()).ToList();

                files.ForEach(file => TryRemoveFile(fileKeys, file));
            }
        }

        private void TryRemoveFile(List<string> fileKeys, KeyValuePair<string, FileEntry> file)
        {
            lock (_filesLock)
            {
                if ((fileKeys.Contains(file.Key) == false) && (HasValidEvents(file.Value) == false) && (IsProtected(file.Value) == false))
                {
                    Files.Remove(file.Key);
                }
            }
        }

        private void OnFileDeleted(object sender
            , System.IO.FileSystemEventArgs e)
        {
            lock (_filesLock)
            {
                string key = e.FullPath.ToLower();

                if ((Files.TryGetValue(key, out var entry)) && (HasValidEvents(entry) == false) && (IsProtected(entry) == false))
                {
                    Files.Remove(key);
                }
            }

            RaiseFilesChanged();
        }

        private void OnFileCreated(object sender
            , System.IO.FileSystemEventArgs e)
        {
            OnFileCreated(e, new FileEntry());

            RaiseFilesChanged();
        }

        private void OnFileCreated(System.IO.FileSystemEventArgs e
            , FileEntry entry)
        {
            OnFileCreated(e.FullPath, entry);
        }

        private void OnFileCreated(string fileName, FileEntry entry)
        {
            lock (_filesLock)
            {
                string key = entry.Key;

                if (Files.ContainsKey(key) == false)
                {
                    entry.FullName = fileName;

                    Files.Add(key, entry);
                }
            }
        }

        private void RaiseFilesChanged()
            => _FilesChanged?.Invoke(this, EventArgs.Empty);

        private void MergeEntry(FileEntry existingEntry, FileEntry newEntry)
        {
            existingEntry.Title = newEntry.Title;

            if (newEntry.VideoLength > 0)
            {
                existingEntry.VideoLength = newEntry.VideoLength;
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
            var existingUser = existingEntry.Users.FirstOrDefault(eu => eu.UserName == newUser.UserName);

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

            existingUser.Watches = existingUser.Watches.Union(newUser.Watches).ToArray();
        }
    }
}