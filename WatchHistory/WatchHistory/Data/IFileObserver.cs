namespace DoenaSoft.WatchHistory.Data
{
    using System;
    using System.Collections.Generic;


    internal interface IFileObserver
    {
        event System.IO.FileSystemEventHandler Created;

        event System.IO.FileSystemEventHandler Deleted;

        event System.IO.RenamedEventHandler Renamed;

        void Observe(IEnumerable<String> rootFolders
            , IEnumerable<String> fileExtensions);

        void Suspend();
    }
}