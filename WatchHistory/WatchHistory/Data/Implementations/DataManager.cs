namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using AbstractionLayer.IOServices;
    using DVDProfiler.DVDProfilerHelper;
    using ToolBox.Extensions;
    using WatchHistory.Implementations;
    using WatchHistory.Main.Implementations;

    internal sealed class DataManager : IDataManager
    {
        private readonly IIOServices IOServices;

        private readonly IFileObserver FileObserver;

        private readonly Object FilesLock;

        private readonly IFilesSerializer FilesSerializer;

        private IEnumerable<String> m_RootFolders;

        private IEnumerable<String> m_FileExtensions;

        private IEnumerable<String> m_Users;

        private Dictionary<String, FileEntry> Files { get; set; }

        private Boolean IsSuspended { get; set; }

        private event EventHandler m_FilesChanged;

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
            get
            {
                foreach (String rootFolder in m_RootFolders)
                {
                    yield return (rootFolder);
                }
            }
            set
            {
                value = new HashSet<String>(value);

                FileObserver.Observe(value, m_FileExtensions);

                m_RootFolders = value.ToList();

                SyncData();
            }
        }

        public IEnumerable<String> FileExtensions
        {
            get
            {
                foreach (String fileExtension in m_FileExtensions)
                {
                    yield return (fileExtension);
                }
            }
            set
            {
                value = value.ForEach(FileNameHelper.GetInstance(IOServices).ReplaceInvalidFileNameChars);

                value = new HashSet<String>(value);

                FileObserver.Observe(m_RootFolders, value);

                m_FileExtensions = value.ToList();

                SyncData();
            }
        }

        public IEnumerable<String> Users
        {
            get
            {
                foreach (String user in m_Users)
                {
                    yield return (user);
                }
            }
            set
            {
                value = new HashSet<String>(value);

                m_Users = value.ToList();
            }
        }

        public event EventHandler FilesChanged
        {
            add
            {
                if (m_FilesChanged == null)
                {
                    FileObserver.Created += OnFileCreated;
                    FileObserver.Deleted += OnFileDeleted;
                    FileObserver.Renamed += OnFileRenamed;
                }

                m_FilesChanged += value;
            }
            remove
            {
                m_FilesChanged -= value;

                if (m_FilesChanged == null)
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
            User user = TryGetUser(entry, userName);

            user = user ?? AddUser(entry, userName);

            user.Ignore = true;
            user.IgnoreSpecified = true;

            RaiseFilesChanged();
        }

        public void UndoIgnore(FileEntry entry
            , String userName)
        {
            User user = TryGetUser(entry, userName);

            if (user != null)
            {
                user.Ignore = false;
                user.IgnoreSpecified = false;

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
                Users = m_Users.ToArray(),
                RootFolders = m_RootFolders.ToArray(),
                FileExtensions = m_FileExtensions.ToArray()
            };

            Settings settings = new Settings()
            {
                DefaultValues = defaultValues
            };

            using (Stream fs = IOServices.GetFileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                Serializer<Settings>.Serialize(fs, settings);
            }
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

            FileObserver.Observe(m_RootFolders, m_FileExtensions);

            SyncData();
        }

        #endregion

        private void LoadSettings(String fileName)
        {
            Settings settings;
            try
            {
                using (Stream fs = IOServices.GetFileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    settings = Serializer<Settings>.Deserialize(fs);
                }
            }
            catch
            {
                settings = new Settings()
                {
                    DefaultValues = new DefaultValues()
                };
            }

            m_Users = settings?.DefaultValues?.Users ?? Enumerable.Empty<String>();

            m_RootFolders = settings?.DefaultValues?.RootFolders ?? Enumerable.Empty<String>();

            m_FileExtensions = settings?.DefaultValues?.FileExtensions ?? Enumerable.Empty<String>();
        }

        private void LoadData(String fileName)
        {
            Files files = FilesSerializer.LoadData(fileName);

            lock (FilesLock)
            {
                Files = new Dictionary<String, FileEntry>(files?.Entries?.Length ?? 0);

                IEnumerable<FileEntry> entries = files?.Entries ?? Enumerable.Empty<FileEntry>();

                foreach (FileEntry entry in entries)
                {
                    Files[entry.Key] = entry;
                }
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
            => (entry.Users?.Where(user => IsUser(user, userName)).FirstOrDefault());

        private static Boolean IsUser(User user
            , String userName)
            => (user.UserName == userName);

        private void SyncData()
        {
            if (IsSuspended == false)
            {
                GetActualFiles();
            }
        }

        private Boolean HasEvents(FileEntry entry)
            => (entry.Users?.HasItemsWhere(HasEvents) == true);

        private static Boolean HasEvents(User user)
            => (user.Watches?.HasItems() == true);

        private void GetActualFiles()
        {
            List<Task<IEnumerable<String>>> tasks = new List<Task<IEnumerable<String>>>();

            foreach (String rootFolder in m_RootFolders)
            {
                foreach (String fileExtension in m_FileExtensions)
                {
                    Task<IEnumerable<String>> task = Task.Run(() => GetFiles(rootFolder, fileExtension));

                    tasks.Add(task);
                }
            }

            Task<IEnumerable<String>[]> readyTask = Task.WhenAll(tasks);

            readyTask.ContinueWith(ProcessActualFiles);
        }

        private void ProcessActualFiles(Task<IEnumerable<String>[]> task)
        {
            IEnumerable<String> actualFiles = task.Result.SelectMany(file => file).ToList();

            lock (FilesLock)
            {
                AddActualFiles(actualFiles);

                RemoveObsoletesFiles(actualFiles);
            }

            RaiseFilesChanged();
        }

        private IEnumerable<String> GetFiles(String rootFolder
            , String fileExtension)
            => (IOServices.Folder.Exists(rootFolder)
                ? (IOServices.Folder.GetFiles(rootFolder, "*." + fileExtension, SearchOption.AllDirectories))
                : (Enumerable.Empty<String>()));

        private void AddActualFiles(IEnumerable<String> actualFiles)
        {
            foreach (String actualFile in actualFiles)
            {
                AddActualFile(actualFile);
            }
        }

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
                (new DvdWatchesProcessor()).UpdateFromDvdWatches(entry);
            }
        }

        private void RemoveObsoletesFiles(IEnumerable<String> actualFiles)
        {
            List<KeyValuePair<String, FileEntry>> files = Files.ToList();

            List<String> fileKeys = actualFiles.ForEach(f => f.ToLower()).ToList();

            foreach (KeyValuePair<String, FileEntry> kvp in files)
            {
                if ((fileKeys.Contains(kvp.Key) == false) && (HasEvents(kvp.Value) == false))
                {
                    Files.Remove(kvp.Key);
                }
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
        {
            m_FilesChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}