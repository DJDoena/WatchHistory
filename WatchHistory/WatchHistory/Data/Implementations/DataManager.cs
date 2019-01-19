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
        private readonly IIOServices _IOServices;

        private readonly IFileObserver _FileObserver;

        private readonly Object _FilesLock;

        private readonly IFilesSerializer _FilesSerializer;

        private readonly String _DataFile;

        private readonly String _SettingsFile;

        private static readonly DateTime _TurnOfTheCentury;

        private IEnumerable<String> _RootFolders;

        private IEnumerable<String> _FileExtensions;

        private IEnumerable<String> _Users;

        private Boolean _IsSynchronizing;

        private Dictionary<String, FileEntry> Files { get; set; }

        private Boolean IsSuspended { get; set; }

        private event EventHandler _FilesChanged;

        static DataManager()
        {
            _TurnOfTheCentury = new DateTime(1999, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        }

        public DataManager(String settingsFile
            , String dataFile
            , IIOServices ioServices)
        {
            _IOServices = ioServices;

            _FilesLock = new Object();

            _FileObserver = new FileObserver(ioServices);

            _SettingsFile = settingsFile;

            LoadSettings();

            _FilesSerializer = new FilesSerializer(ioServices);

            _FilesSerializer.CreateBackup(dataFile);

            _DataFile = dataFile;

            LoadData();
        }

        #region IDataManager

        public IEnumerable<String> RootFolders
        {
            get => _RootFolders.Select(folder => folder);
            set
            {
                value = new HashSet<String>(value);

                _FileObserver.Observe(value, _FileExtensions);

                _RootFolders = value.ToList();

                SyncData();
            }
        }

        public IEnumerable<String> FileExtensions
        {
            get => _FileExtensions.Select(ext => ext);
            set
            {
                value = value.Select(ext => ext.ReplaceInvalidFileNameChars('_'));

                value = new HashSet<String>(value);

                _FileObserver.Observe(_RootFolders, value);

                _FileExtensions = value.ToList();

                SyncData();
            }
        }

        public IEnumerable<String> Users
        {
            get => _Users.Select(user => user);
            set
            {
                value = new HashSet<String>(value);

                _Users = value.ToList();
            }
        }

        public Boolean IsSynchronizing
        {
            get => _IsSynchronizing;
            private set
            {
                _IsSynchronizing = value;

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
                    _FileObserver.Created += OnFileCreated;
                    _FileObserver.Deleted += OnFileDeleted;
                }

                _FilesChanged += value;
            }
            remove
            {
                _FilesChanged -= value;

                if (_FilesChanged == null)
                {
                    _FileObserver.Created -= OnFileCreated;
                    _FileObserver.Deleted -= OnFileDeleted;
                }
            }
        }

        public IEnumerable<FileEntry> GetFiles()
        {
            lock (_FilesLock)
            {
                return (Files.Values);
            }
        }

        public void AddWatched(FileEntry entry
           , String userName)
            => AddWatched(entry, userName, DateTime.UtcNow);

        public void AddWatched(FileEntry entry
            , String userName
            , DateTime watchedOn)
        {
            User user = TryGetUser(entry, userName);

            user = user ?? AddUser(entry, userName);

            AddWatched(user, watchedOn);
        }

        public void AddIgnore(FileEntry entry
            , String userName)
        {
            User user = TryGetUser(entry, userName) ?? AddUser(entry, userName);

            user.Ignore = true;

            RaiseFilesChanged();
        }

        public void UndoIgnore(FileEntry entry
            , String userName)
        {
            User user = TryGetUser(entry, userName);

            if (user != null)
            {
                user.Ignore = false;

                RaiseFilesChanged();
            }
        }

        public DateTime GetLastWatched(FileEntry entry
            , String userName)
        {
            User user = TryGetUser(entry, userName);

            DateTime lastWatched = new DateTime(0);

            if (user?.Watches?.HasItems() == true)
            {
                lastWatched = user.Watches.Max(watch => watch.Value).ToLocalTime();
            }

            return (lastWatched);
        }

        public DateTime GetCreationTime(FileEntry fileEntry)
        {
            IFileInfo fi = _IOServices.GetFileInfo(fileEntry.FullName);

            DateTime creationTime = new DateTime(0);

            if (fi.Exists)
            {
                creationTime = fi.CreationTime;
            }

            return (creationTime);
        }

        public UInt32 GetVideoLength(FileEntry fileEntry)
        {
            try
            {
                VideoReader videoReader = new VideoReader(_IOServices, fileEntry);

                UInt32 videoLength = videoReader.GetLength();

                return videoLength;
            }
            catch
            {
                return 0;
            }
        }
        public void SaveSettingsFile()
        {
            DefaultValues defaultValues = new DefaultValues()
            {
                Users = _Users.ToArray(),
                RootFolders = _RootFolders.ToArray(),
                FileExtensions = _FileExtensions.ToArray()
            };

            Settings settings = new Settings()
            {
                DefaultValues = defaultValues
            };

            SerializerHelper.Serialize(_IOServices, _SettingsFile, settings);
        }

        public void SaveDataFile()
        {
            lock (_FilesLock)
            {
                List<FileEntry> entries = Files.Values.ToList();

                entries.Sort((left, right) => left.FullName.CompareTo(right.FullName));

                Files files = new Files()
                {
                    Entries = entries.ToArray()
                };

                _FilesSerializer.SaveFile(_DataFile, files);
            }
        }

        public void Suspend()
        {
            IsSuspended = true;

            _FileObserver.Suspend();
        }

        public void Resume()
        {
            IsSuspended = false;

            _FileObserver.Observe(_RootFolders, _FileExtensions);

            SyncData();
        }

        #endregion

        private void LoadSettings()
        {
            Settings settings;
            try
            {
                settings = SerializerHelper.Deserialize<Settings>(_IOServices, _SettingsFile);
            }
            catch
            {
                settings = new Settings()
                {
                    DefaultValues = new DefaultValues()
                };
            }

            _Users = settings?.DefaultValues?.Users ?? Enumerable.Empty<String>();

            _RootFolders = settings?.DefaultValues?.RootFolders ?? Enumerable.Empty<String>();

            _FileExtensions = settings?.DefaultValues?.FileExtensions ?? Enumerable.Empty<String>();
        }

        private void LoadData()
        {
            Files files = _FilesSerializer.LoadData(_DataFile);

            lock (_FilesLock)
            {
                Files = new Dictionary<String, FileEntry>(files?.Entries?.Length ?? 0);

                IEnumerable<FileEntry> entries = files?.Entries ?? Enumerable.Empty<FileEntry>();

                entries.ForEach(entry => Files[entry.Key] = entry);
            }
        }

        private User AddUser(FileEntry entry
            , String userName)
        {
            List<User> users = entry.Users?.ToList() ?? new List<User>(1);

            User user = new User()
            {
                UserName = userName
            };

            users.Add(user);

            entry.Users = users.ToArray();

            return (user);
        }

        private void AddWatched(User user
            , DateTime watchedOn)
        {
            List<Watch> watches = user.Watches?.ToList() ?? new List<Watch>(1);

            watches.Add(new Watch() { Value = watchedOn });

            user.Watches = watches.ToArray();

            RaiseFilesChanged();
        }

        private static User TryGetUser(FileEntry entry
            , String userName)
            => entry.Users?.Where(user => IsUser(user, userName)).FirstOrDefault();

        private static Boolean IsUser(User user
            , String userName)
            => user.UserName == userName;

        private void SyncData()
        {
            if (IsSuspended == false)
            {
                GetActualFiles();
            }
        }

        private Boolean HasValidEvents(FileEntry entry)
            => entry.Users?.HasItemsWhere(HasEvents) == true;

        private static Boolean HasEvents(User user)
            => user.Watches?.HasItemsWhere(w => w.SourceSpecified == false && w.Value > _TurnOfTheCentury) == true;

        private void GetActualFiles()
        {
            IsSynchronizing = true;

            IEnumerable<Task<IEnumerable<String>>> tasks = _RootFolders.Select(GetActualFiles).SelectMany(task => task);

            Task<IEnumerable<String>[]> readyTask = Task.WhenAll(tasks);

            readyTask.ContinueWith(ProcessActualFiles);
        }

        private IEnumerable<Task<IEnumerable<String>>> GetActualFiles(String folder)
            => _FileExtensions.Select(ext => GetActualFiles(folder, ext));

        private Task<IEnumerable<String>> GetActualFiles(String folder, String fileExtension)
            => Task.Run(() => GetFiles(folder, fileExtension));

        private void ProcessActualFiles(Task<IEnumerable<String>[]> task)
        {
            IEnumerable<String> actualFiles = task.Result.SelectMany(file => file).ToList();

            lock (_FilesLock)
            {
                actualFiles.ForEach(AddActualFile);

                RemoveObsoletesFiles(actualFiles);

                SaveDataFile();
            }

            IsSynchronizing = false;

            RaiseFilesChanged();
        }

        private IEnumerable<String> GetFiles(String rootFolder
            , String fileExtension)
            => _IOServices.Folder.Exists(rootFolder)
                ? (_IOServices.Folder.GetFiles(rootFolder, "*." + fileExtension, System.IO.SearchOption.AllDirectories))
                : (Enumerable.Empty<String>());

        private void AddActualFile(String actualFile)
        {
            String key = actualFile.ToLower();

            if (Files.TryGetValue(key, out FileEntry entry) == false)
            {
                entry = new FileEntry()
                {
                    FullName = actualFile,
                };

                entry.CreationTime = entry.GetCreationTime(this);

                Files.Add(key, entry);
            }

            if (entry.TitleSpecified == false)
            {
                (new VideoInfoAdder(_IOServices, entry)).Add();
            }

            if (actualFile.EndsWith(Constants.DvdProfilerFileExtension))
            {
                (new DvdWatchesProcessor(_IOServices)).UpdateFromDvdWatches(entry);
            }
        }

        private void RemoveObsoletesFiles(IEnumerable<String> actualFiles)
        {
            List<KeyValuePair<String, FileEntry>> files = Files.ToList();

            List<String> fileKeys = actualFiles.Select(file => file.ToLower()).ToList();

            files.ForEach(file => TryRemoveFile(fileKeys, file));
        }

        private void TryRemoveFile(List<String> fileKeys, KeyValuePair<String, FileEntry> file)
        {
            if ((fileKeys.Contains(file.Key) == false) && (HasValidEvents(file.Value) == false))
            {
                Files.Remove(file.Key);
            }
        }

        private void OnFileDeleted(Object sender
            , System.IO.FileSystemEventArgs e)
        {
            lock (_FilesLock)
            {
                String key = e.FullPath.ToLower();

                if ((Files.TryGetValue(key, out FileEntry entry)) && (HasValidEvents(entry) == false))
                {
                    Files.Remove(key);
                }
            }

            RaiseFilesChanged();
        }

        private void OnFileCreated(Object sender
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

        private void OnFileCreated(String fileName, FileEntry entry)
        {
            lock (_FilesLock)
            {
                String key = fileName.ToLower();

                if (Files.ContainsKey(key) == false)
                {
                    entry.FullName = fileName;

                    Files.Add(key, entry);
                }
            }
        }

        private void RaiseFilesChanged()
            => _FilesChanged?.Invoke(this, EventArgs.Empty);
    }
}