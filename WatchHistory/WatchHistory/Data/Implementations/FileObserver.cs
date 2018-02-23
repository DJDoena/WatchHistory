namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AbstractionLayer.IOServices;
    using ToolBox.Extensions;

    internal sealed class FileObserver : IFileObserver
    {
        private readonly IIOServices _IOServices;

        private Dictionary<String, Dictionary<String, IFileSystemWatcher>> _FileSystemWatchers;

        private event System.IO.FileSystemEventHandler _Created;

        private event System.IO.FileSystemEventHandler _Deleted;

        private event System.IO.RenamedEventHandler _Renamed;

        private Boolean IsSuspended { get; set; }

        public FileObserver(IIOServices ioServices)
        {
            _IOServices = ioServices;
        }

        #region IFileObserver

        public event System.IO.FileSystemEventHandler Created
        {
            add
            {
                if (_Created == null)
                {
                    GetFileSystemWatchers().ForEach(fsw => fsw.Created += OnFileCreated);
                }

                _Created += value;
            }
            remove
            {
                _Created -= value;

                if (_Created == null)
                {
                    GetFileSystemWatchers().ForEach(fsw => fsw.Created -= OnFileCreated);
                }
            }
        }

        public event System.IO.FileSystemEventHandler Deleted
        {
            add
            {
                if (_Deleted == null)
                {
                    GetFileSystemWatchers().ForEach(fsw => fsw.Deleted += OnFileDeleted);
                }

                _Deleted += value;
            }
            remove
            {
                _Deleted -= value;

                if (_Deleted == null)
                {
                    GetFileSystemWatchers().ForEach(fsw => fsw.Deleted -= OnFileDeleted);
                }
            }
        }

        public event System.IO.RenamedEventHandler Renamed
        {
            add
            {
                if (_Renamed == null)
                {
                    GetFileSystemWatchers().ForEach(fsw => fsw.Renamed += OnFileRenamed);
                }

                _Renamed += value;
            }
            remove
            {
                _Renamed -= value;

                if (_Renamed == null)
                {
                    GetFileSystemWatchers().ForEach(fsw => fsw.Renamed -= OnFileRenamed);
                }
            }
        }

        public void Observe(IEnumerable<String> rootFolders
           , IEnumerable<String> fileExtensions)
        {
            Dictionary<String, Dictionary<String, IFileSystemWatcher>> watchers = new Dictionary<String, Dictionary<String, IFileSystemWatcher>>();

            rootFolders.ForEach(folder => CreateWatchers(folder, fileExtensions, watchers));

            DisposeFileSystemWatchers();

            _FileSystemWatchers = watchers;
        }

        public void Suspend()
        {
            DisposeFileSystemWatchers();

            _FileSystemWatchers = null;
        }

        #endregion

        private void CreateWatchers(String rootFolder
            , IEnumerable<String> fileExtensions
            , Dictionary<String, Dictionary<String, IFileSystemWatcher>> byFolderWatchers)
        {
            if (_IOServices.Folder.Exists(rootFolder))
            {
                Dictionary<String, IFileSystemWatcher> byExtensionWatchers = CreateWatchers(rootFolder, fileExtensions);

                byFolderWatchers.Add(rootFolder, byExtensionWatchers);
            }
        }

        private Dictionary<String, IFileSystemWatcher> CreateWatchers(String rootFolder
            , IEnumerable<String> fileExtensions)
        {
            Dictionary<String, IFileSystemWatcher> watchers = new Dictionary<String, IFileSystemWatcher>();

            fileExtensions.ForEach(ext => AddWatcher(watchers, rootFolder, ext));

            return (watchers);
        }

        private void AddWatcher(Dictionary<String, IFileSystemWatcher> watchers, String rootFolder, String fileExtension)
        {
            IFileSystemWatcher fsw = CreateWatcher(rootFolder, fileExtension);

            watchers.Add(fileExtension, fsw);
        }

        private IFileSystemWatcher CreateWatcher(String rootFolder
            , String fileExtension)
        {
            IFileSystemWatcher fsw = _IOServices.GetFileSystemWatcher(rootFolder, "*." + fileExtension);

            if (_Created != null)
            {
                fsw.Created += OnFileCreated;
            }

            if (_Deleted != null)
            {
                fsw.Deleted += OnFileDeleted;
            }

            if (_Renamed != null)
            {
                fsw.Renamed += OnFileRenamed;
            }

            fsw.EnableRaisingEvents = (IsSuspended == false);
            fsw.IncludeSubFolders = true;

            return (fsw);
        }

        private void DisposeFileSystemWatchers()
            => GetFileSystemWatchers().ForEach(DisposeFileSystemWatcher);

        private void DisposeFileSystemWatcher(IFileSystemWatcher fsw)
        {
            if (_Created != null)
            {
                fsw.Created -= OnFileCreated;
            }

            if (_Deleted != null)
            {
                fsw.Deleted -= OnFileDeleted;
            }

            if (_Renamed != null)
            {
                fsw.Renamed -= OnFileRenamed;
            }

            fsw.EnableRaisingEvents = false;
        }

        private IEnumerable<IFileSystemWatcher> GetFileSystemWatchers()
            => _FileSystemWatchers?.Values.SelectMany(kvp => kvp.Values) ?? Enumerable.Empty<IFileSystemWatcher>();

        private void OnFileRenamed(Object sender
            , System.IO.RenamedEventArgs e)
            => _Renamed?.Invoke(this, e);

        private void OnFileDeleted(Object sender
            , System.IO.FileSystemEventArgs e)
            => _Deleted?.Invoke(this, e);

        private void OnFileCreated(Object sender
            , System.IO.FileSystemEventArgs e)
            => _Created?.Invoke(this, e);
    }
}