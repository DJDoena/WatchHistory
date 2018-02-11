namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using AbstractionLayer.IOServices;
    using ToolBox.Extensions;
    using WatchHistory.Implementations;

    internal sealed class DataManager : IDataManager
    {
        private readonly IIOServices IOServices;

        private readonly IFileObserver FileObserver;

        private readonly Object FilesLock;

        private readonly IFilesSerializer FilesSerializer;

        private IEnumerable<String> _RootFolders;

        private IEnumerable<String> _FileExtensions;

        private IEnumerable<String> _Users;

        private Dictionary<String, FileEntry> Files { get; set; }

        private Boolean IsSuspended { get; set; }

        private event EventHandler _FilesChanged;

        public DataManager(String settingsFile
            , String dataFile
            , IIOServices ioServices)
        {
            IOServices = ioServices;

            FilesLock = new Object();

            FileObserver = new FileObserver(ioServices);

            LoadSettings(settingsFile);

            FilesSerializer = new FilesSerializer(ioServices);

            FilesSerializer.CreateBackup(dataFile);

            LoadData(dataFile);
        }

        #region IDataManager

        public IEnumerable<String> RootFolders
        {
            get => _RootFolders.Select(folder => folder);
            set
            {
                value = new HashSet<String>(value);

                FileObserver.Observe(value, _FileExtensions);

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

                FileObserver.Observe(_RootFolders, value);

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

        public event EventHandler FilesChanged
        {
            add
            {
                if (_FilesChanged == null)
                {
                    FileObserver.Created += OnFileCreated;
                    FileObserver.Deleted += OnFileDeleted;
                    FileObserver.Renamed += OnFileRenamed;
                }

                _FilesChanged += value;
            }
            remove
            {
                _FilesChanged -= value;

                if (_FilesChanged == null)
                {
                    FileObserver.Created -= OnFileCreated;
                    FileObserver.Deleted -= OnFileDeleted;
                    FileObserver.Renamed -= OnFileRenamed;
                }
            }
        }

        public IEnumerable<FileEntry> GetFiles()
        {
            lock (FilesLock)
            {
                return (Files.Values);
            }
        }

        public void AddWatched(FileEntry entry
            , String userName)
        {
            User user = TryGetUser(entry, userName);

            user = user ?? AddUser(entry, userName);

            AddWatched(user);
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
            IFileInfo fi = IOServices.GetFileInfo(fileEntry.FullName);

            DateTime creationTime = new DateTime(0);

            if (fi.Exists)
            {
                creationTime = fi.CreationTime;
            }

            return (creationTime);
        }

        public void SaveSettingsFile(String fileName)
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

            SerializerHelper.Serialize(IOServices, fileName, settings);
        }

        public void SaveDataFile(String fileName)
        {
            lock (FilesLock)
            {
                List<FileEntry> entries = Files.Values.ToList();

                entries.Sort((left, right) => left.FullName.CompareTo(right.FullName));

                Files files = new Files()
                {
                    Entries = entries.ToArray()
                };

                FilesSerializer.SaveFile(fileName, files);
            }
        }

        public void Suspend()
        {
            IsSuspended = true;

            FileObserver.Suspend();
        }

        public void Resume()
        {
            IsSuspended = false;

            FileObserver.Observe(_RootFolders, _FileExtensions);

            SyncData();
        }

        #endregion

        private void LoadSettings(String fileName)
        {
            Settings settings;
            try
            {
                settings = SerializerHelper.Deserialize<Settings>(IOServices, fileName);
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

        private void LoadData(String fileName)
        {
            Files files = FilesSerializer.LoadData(fileName);

            lock (FilesLock)
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

        private void AddWatched(User user)
        {
            List<Watch> watches = user.Watches?.ToList() ?? new List<Watch>(1);

            watches.Add(new Watch() { Value = DateTime.UtcNow });

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

        private Boolean HasEvents(FileEntry entry)
            => entry.Users?.HasItemsWhere(HasEvents) == true;

        private static Boolean HasEvents(User user)
            => user.Watches?.HasItems() == true;

        private void GetActualFiles()
        {
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

            lock (FilesLock)
            {
                actualFiles.ForEach(AddActualFile);

                RemoveObsoletesFiles(actualFiles);
            }

            RaiseFilesChanged();
        }

        private IEnumerable<String> GetFiles(String rootFolder
            , String fileExtension)
            => IOServices.Folder.Exists(rootFolder)
                ? (IOServices.Folder.GetFiles(rootFolder, "*." + fileExtension, SearchOption.AllDirectories))
                : (Enumerable.Empty<String>());

        private void AddActualFile(String actualFile)
        {
            String key = actualFile.ToLower();

            if (Files.TryGetValue(key, out FileEntry entry) == false)
            {
                entry = new FileEntry()
                {
                    FullName = actualFile,
                    CreationTime = entry.GetCreationTime(this)
                };

                Files.Add(key, entry);
            }

            if (actualFile.EndsWith("." + Constants.DvdProfilerFileExtension))
            {
                (new DvdWatchesProcessor(IOServices)).UpdateFromDvdWatches(entry);
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
            if ((fileKeys.Contains(file.Key) == false) && (HasEvents(file.Value) == false))
            {
                Files.Remove(file.Key);
            }
        }

        private void OnFileRenamed(Object sender
            , RenamedEventArgs e)
        {
            FileEntry entry = new FileEntry();

            String fileName = e.OldFullPath;

            lock (FilesLock)
            {
                String key = fileName.ToLower();

                if (Files.TryGetValue(key, out entry))
                {
                    Files.Remove(key);
                }

                OnFileCreated(fileName, entry);
            }

            RaiseFilesChanged();
        }

        private void OnFileDeleted(Object sender
            , FileSystemEventArgs e)
        {
            lock (FilesLock)
            {
                String key = e.FullPath.ToLower();

                if ((Files.TryGetValue(key, out FileEntry entry)) && (HasEvents(entry) == false))
                {
                    Files.Remove(key);
                }
            }

            RaiseFilesChanged();
        }

        private void OnFileCreated(Object sender
            , FileSystemEventArgs e)
        {
            OnFileCreated(e, new FileEntry());

            RaiseFilesChanged();
        }

        private void OnFileCreated(FileSystemEventArgs e
            , FileEntry entry)
        {
            OnFileCreated(e.FullPath, entry);
        }

        private void OnFileCreated(String fileName, FileEntry entry)
        {
            lock (FilesLock)
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