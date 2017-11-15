﻿namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AbstractionLayer.IOServices;

    internal sealed class FileObserver : IFileObserver
    {
        private readonly IIOServices IOServices;

        private Dictionary<String, Dictionary<String, IFileSystemWatcher>> m_FileSystemWatchers;

        private event System.IO.FileSystemEventHandler m_Created;

        private event System.IO.FileSystemEventHandler m_Deleted;

        private event System.IO.RenamedEventHandler m_Renamed;

        private Boolean IsSuspended { get; set; }

        public FileObserver(IIOServices ioServices)
        {
            IOServices = ioServices;
        }

        #region IFileObserver

        public event System.IO.FileSystemEventHandler Created
        {
            add
            {
                if (m_Created == null)
                {
                    foreach (IFileSystemWatcher fsw in GetFileSystemWatchers())
                    {
                        fsw.Created += OnFileCreated;
                    }
                }

                m_Created += value;
            }
            remove
            {
                m_Created -= value;

                if (m_Created == null)
                {
                    foreach (IFileSystemWatcher fsw in GetFileSystemWatchers())
                    {
                        fsw.Created -= OnFileCreated;
                    }
                }
            }
        }

        public event System.IO.FileSystemEventHandler Deleted
        {
            add
            {
                if (m_Deleted == null)
                {
                    foreach (IFileSystemWatcher fsw in GetFileSystemWatchers())
                    {
                        fsw.Deleted += OnFileDeleted;
                    }
                }

                m_Deleted += value;
            }
            remove
            {
                m_Deleted -= value;

                if (m_Deleted == null)
                {
                    foreach (IFileSystemWatcher fsw in GetFileSystemWatchers())
                    {
                        fsw.Deleted -= OnFileDeleted;
                    }
                }
            }
        }

        public event System.IO.RenamedEventHandler Renamed
        {
            add
            {
                if (m_Renamed == null)
                {
                    foreach (IFileSystemWatcher fsw in GetFileSystemWatchers())
                    {
                        fsw.Renamed += OnFileRenamed;
                    }
                }

                m_Renamed += value;
            }
            remove
            {
                m_Renamed -= value;

                if (m_Renamed == null)
                {
                    foreach (IFileSystemWatcher fsw in GetFileSystemWatchers())
                    {
                        fsw.Renamed -= OnFileRenamed;
                    }
                }
            }
        }

        public void Observe(IEnumerable<String> rootFolders
           , IEnumerable<String> fileExtensions)
        {
            Dictionary<String, Dictionary<String, IFileSystemWatcher>> watchers = new Dictionary<String, Dictionary<String, IFileSystemWatcher>>();

            foreach (String rootFolder in rootFolders)
            {
                CreateWatchers(rootFolder, fileExtensions, watchers);
            }

            DisposeFileSystemWatchers();

            m_FileSystemWatchers = watchers;
        }

        public void Suspend()
        {
            DisposeFileSystemWatchers();

            m_FileSystemWatchers = null;
        }

        #endregion

        private void CreateWatchers(String rootFolder
            , IEnumerable<String> fileExtensions
            , Dictionary<String, Dictionary<String, IFileSystemWatcher>> byFolderWatchers)
        {
            if (IOServices.Directory.Exists(rootFolder))
            {
                Dictionary<String, IFileSystemWatcher> byExtensionWatchers = CreateWatchers(rootFolder, fileExtensions);

                byFolderWatchers.Add(rootFolder, byExtensionWatchers);
            }
        }

        private Dictionary<String, IFileSystemWatcher> CreateWatchers(String rootFolder
            , IEnumerable<String> fileExtensions)
        {
            Dictionary<String, IFileSystemWatcher> watchers = new Dictionary<String, IFileSystemWatcher>();

            foreach (String fileExtension in fileExtensions)
            {
                IFileSystemWatcher fsw = CreateWatcher(rootFolder, fileExtension);

                watchers.Add(fileExtension, fsw);
            }

            return (watchers);
        }

        private IFileSystemWatcher CreateWatcher(String rootFolder
            , String fileExtension)
        {
            IFileSystemWatcher fsw = IOServices.GetFileSystemWatcher(rootFolder, "*." + fileExtension);

            if (m_Created != null)
            {
                fsw.Created += OnFileCreated;
            }

            if (m_Deleted != null)
            {
                fsw.Deleted += OnFileDeleted;
            }

            if (m_Renamed != null)
            {
                fsw.Renamed += OnFileRenamed;
            }

            fsw.EnableRaisingEvents = (IsSuspended == false);
            fsw.IncludeSubdirectories = true;

            return (fsw);
        }

        private void DisposeFileSystemWatchers()
        {
            foreach (IFileSystemWatcher fsw in GetFileSystemWatchers())
            {
                DisposeFileSystemWatcher(fsw);
            }
        }

        private void DisposeFileSystemWatcher(IFileSystemWatcher fsw)
        {
            if (m_Created != null)
            {
                fsw.Created -= OnFileCreated;
            }

            if (m_Deleted != null)
            {
                fsw.Deleted -= OnFileDeleted;
            }

            if (m_Renamed != null)
            {
                fsw.Renamed -= OnFileRenamed;
            }

            fsw.EnableRaisingEvents = false;
        }

        private IEnumerable<IFileSystemWatcher> GetFileSystemWatchers()
            => (m_FileSystemWatchers?.Values.SelectMany(kvp => kvp.Values) ?? Enumerable.Empty<IFileSystemWatcher>());

        private void OnFileRenamed(Object sender
            , System.IO.RenamedEventArgs e)
        {
            m_Renamed?.Invoke(this, e);
        }

        private void OnFileDeleted(Object sender
            , System.IO.FileSystemEventArgs e)
        {
            m_Deleted?.Invoke(this, e);
        }

        private void OnFileCreated(Object sender
            , System.IO.FileSystemEventArgs e)
        {
            m_Created?.Invoke(this, e);
        } 
    }
}