namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AbstractionLayer.IOServices;
    using DVDProfiler.DVDProfilerHelper;
    using ToolBox.Extensions;
    using WatchHistory.Implementations;

    internal sealed class DataManager : IDataManager
    {
        private readonly IIOServices IOServices;

        private readonly IFileObserver FileObserver;

        private readonly Object FilesLock;

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
                value = value.Select(FileNameHelper.GetInstance(IOServices).ReplaceInvalidFileNameChars);

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
                lastWatched = user.Watches.Max();
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

        public void SaveSettingsFile(String file)
        {
            Settings settings = new Settings();

            settings.DefaultValues = new DefaultValues();

            settings.DefaultValues.Users = m_Users.ToArray();
            settings.DefaultValues.RootFolders = m_RootFolders.ToArray();
            settings.DefaultValues.FileExtensions = m_FileExtensions.ToArray();

            Serializer<Settings>.Serialize(file, settings);
        }

        public void SaveDataFile(String file)
        {
            Files files = new Files();

            lock (FilesLock)
            {
                files.Entries = Files.Values.ToArray();
            }

            Serializer<Files>.Serialize(file, files);
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

        private void LoadSettings(String settingsFile)
        {
            Settings settings;
            try
            {
                settings = Serializer<Settings>.Deserialize(settingsFile);
            }
            catch
            {
                settings = new Settings();

                settings.DefaultValues = new DefaultValues();
            }

            m_Users = settings?.DefaultValues?.Users ?? Enumerable.Empty<String>();

            m_RootFolders = settings?.DefaultValues?.RootFolders ?? Enumerable.Empty<String>();

            m_FileExtensions = settings?.DefaultValues?.FileExtensions ?? Enumerable.Empty<String>();
        }

        private void LoadData(String dataFile)
        {
            Files files;
            try
            {
                files = Serializer<Files>.Deserialize(dataFile);
            }
            catch
            {
                files = new Files();
            }

            lock (FilesLock)
            {
                Files = new Dictionary<String, FileEntry>(files?.Entries?.Length ?? 0);

                IEnumerable<FileEntry> entries = files?.Entries ?? Enumerable.Empty<FileEntry>();

                foreach (FileEntry entry in entries)
                {
                    Files[entry.FullName] = entry;
                }
            }
        }

        private User AddUser(FileEntry entry
            , String userName)
        {
            List<User> users = entry.Users?.ToList() ?? new List<User>(1);

            User user = new User();

            user.UserName = userName;

            users.Add(user);

            entry.Users = users.ToArray();

            return (user);
        }

        private void AddWatched(User user)
        {
            List<DateTime> watches = user.Watches?.ToList() ?? new List<DateTime>(1);

            DateTime now = DateTime.Now;

            now = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

            watches.Add(now);

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
        {
            IDirectory dir = IOServices.Directory;

            IEnumerable<String> files;
            if (dir.Exists(rootFolder))
            {
                files = dir.GetFiles(rootFolder, "*." + fileExtension, System.IO.SearchOption.AllDirectories);
            }
            else
            {
                files = Enumerable.Empty<String>();
            }

            return (files);
        }

        private void AddActualFiles(IEnumerable<String> actualFiles)
        {
            foreach (String actualFile in actualFiles)
            {
                if (Files.ContainsKey(actualFile) == false)
                {
                    FileEntry entry = new FileEntry();

                    entry.FullName = actualFile;

                    Files.Add(actualFile, entry);
                }
            }
        }

        private void RemoveObsoletesFiles(IEnumerable<String> actualFiles)
        {
            List<KeyValuePair<String, FileEntry>> files = Files.ToList();

            foreach (KeyValuePair<String, FileEntry> kvp in files)
            {
                if ((actualFiles.Contains(kvp.Key) == false) && (HasEvents(kvp.Value) == false))
                {
                    Files.Remove(kvp.Key);
                }
            }
        }

        private void OnFileRenamed(Object sender
            , System.IO.RenamedEventArgs e)
        {
            FileEntry entry = new FileEntry();

            lock (FilesLock)
            {
                if (Files.TryGetValue(e.OldFullPath, out entry))
                {
                    Files.Remove(e.OldFullPath);
                }
            }

            OnFileCreated(e, entry);

            RaiseFilesChanged();
        }

        private void OnFileDeleted(Object sender
            , System.IO.FileSystemEventArgs e)
        {
            lock (FilesLock)
            {
                Files.Remove(e.FullPath);
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
            lock (FilesLock)
            {
                if (Files.ContainsKey(e.FullPath) == false)
                {
                    entry.FullName = e.FullPath;

                    Files.Add(e.FullPath, entry);
                }
            }
        }

        private void RaiseFilesChanged()
        {
            m_FilesChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}