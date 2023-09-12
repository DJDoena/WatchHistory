namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System.Collections.Generic;
    using System.Linq;
    using AbstractionLayer.IOServices;
    using ToolBox.Extensions;

    internal sealed class FileObserver : IFileObserver
    {
        private readonly IIOServices _ioServices;

        private Dictionary<string, Dictionary<string, IFileSystemWatcher>> _fileSystemWatchers;

#pragma warning disable IDE1006 // Naming Styles
        private event System.IO.FileSystemEventHandler _created;
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
        private event System.IO.FileSystemEventHandler _deleted;
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
        private event System.IO.RenamedEventHandler _renamed;
#pragma warning restore IDE1006 // Naming Styles

        private bool IsSuspended { get; set; }

        public FileObserver(IIOServices ioServices)
        {
            _ioServices = ioServices;
        }

        #region IFileObserver

        public event System.IO.FileSystemEventHandler Created
        {
            add
            {
                if (_created == null)
                {
                    this.GetFileSystemWatchers().ForEach(fsw => fsw.Created += this.OnFileCreated);
                }

                _created += value;
            }
            remove
            {
                _created -= value;

                if (_created == null)
                {
                    this.GetFileSystemWatchers().ForEach(fsw => fsw.Created -= this.OnFileCreated);
                }
            }
        }

        public event System.IO.FileSystemEventHandler Deleted
        {
            add
            {
                if (_deleted == null)
                {
                    this.GetFileSystemWatchers().ForEach(fsw => fsw.Deleted += this.OnFileDeleted);
                }

                _deleted += value;
            }
            remove
            {
                _deleted -= value;

                if (_deleted == null)
                {
                    this.GetFileSystemWatchers().ForEach(fsw => fsw.Deleted -= this.OnFileDeleted);
                }
            }
        }

        public event System.IO.RenamedEventHandler Renamed
        {
            add
            {
                if (_renamed == null)
                {
                    this.GetFileSystemWatchers().ForEach(fsw => fsw.Renamed += this.OnFileRenamed);
                }

                _renamed += value;
            }
            remove
            {
                _renamed -= value;

                if (_renamed == null)
                {
                    this.GetFileSystemWatchers().ForEach(fsw => fsw.Renamed -= this.OnFileRenamed);
                }
            }
        }

        public void Observe(IEnumerable<string> rootFolders
           , IEnumerable<string> fileExtensions)
        {
            var watchers = new Dictionary<string, Dictionary<string, IFileSystemWatcher>>();

            rootFolders.ForEach(folder => this.CreateWatchers(folder, fileExtensions, watchers));

            this.DisposeFileSystemWatchers();

            _fileSystemWatchers = watchers;
        }

        public void Suspend()
        {
            this.DisposeFileSystemWatchers();

            _fileSystemWatchers = null;
        }

        #endregion

        private void CreateWatchers(string rootFolder
            , IEnumerable<string> fileExtensions
            , Dictionary<string, Dictionary<string, IFileSystemWatcher>> byFolderWatchers)
        {
            if (_ioServices.Folder.Exists(rootFolder))
            {
                var byExtensionWatchers = this.CreateWatchers(rootFolder, fileExtensions);

                byFolderWatchers.Add(rootFolder, byExtensionWatchers);
            }
        }

        private Dictionary<string, IFileSystemWatcher> CreateWatchers(string rootFolder
            , IEnumerable<string> fileExtensions)
        {
            var watchers = new Dictionary<string, IFileSystemWatcher>();

            fileExtensions.ForEach(ext => this.AddWatcher(watchers, rootFolder, ext));

            return watchers;
        }

        private void AddWatcher(Dictionary<string, IFileSystemWatcher> watchers, string rootFolder, string fileExtension)
        {
            var fsw = this.CreateWatcher(rootFolder, fileExtension);

            watchers.Add(fileExtension, fsw);
        }

        private IFileSystemWatcher CreateWatcher(string rootFolder, string fileExtension)
        {
            var fsw = _ioServices.GetFileSystemWatcher(rootFolder, "*." + fileExtension);

            if (_created != null)
            {
                fsw.Created += this.OnFileCreated;
            }

            if (_deleted != null)
            {
                fsw.Deleted += this.OnFileDeleted;
            }

            if (_renamed != null)
            {
                fsw.Renamed += this.OnFileRenamed;
            }

            fsw.EnableRaisingEvents = (this.IsSuspended == false);
            fsw.IncludeSubFolders = true;

            return fsw;
        }

        private void DisposeFileSystemWatchers() => this.GetFileSystemWatchers().ForEach(this.DisposeFileSystemWatcher);

        private void DisposeFileSystemWatcher(IFileSystemWatcher fsw)
        {
            if (_created != null)
            {
                fsw.Created -= this.OnFileCreated;
            }

            if (_deleted != null)
            {
                fsw.Deleted -= this.OnFileDeleted;
            }

            if (_renamed != null)
            {
                fsw.Renamed -= this.OnFileRenamed;
            }

            fsw.EnableRaisingEvents = false;
        }

        private IEnumerable<IFileSystemWatcher> GetFileSystemWatchers() => _fileSystemWatchers?.Values.SelectMany(kvp => kvp.Values) ?? Enumerable.Empty<IFileSystemWatcher>();

        private void OnFileRenamed(object sender, System.IO.RenamedEventArgs e) => _renamed?.Invoke(this, e);

        private void OnFileDeleted(object sender, System.IO.FileSystemEventArgs e) => _deleted?.Invoke(this, e);

        private void OnFileCreated(object sender, System.IO.FileSystemEventArgs e) => _created?.Invoke(this, e);
    }
}