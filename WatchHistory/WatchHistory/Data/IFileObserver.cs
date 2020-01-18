namespace DoenaSoft.WatchHistory.Data
{
    using System.Collections.Generic;


    internal interface IFileObserver
    {
        event System.IO.FileSystemEventHandler Created;

        event System.IO.FileSystemEventHandler Deleted;

        event System.IO.RenamedEventHandler Renamed;

        void Observe(IEnumerable<string> rootFolders
            , IEnumerable<string> fileExtensions);

        void Suspend();
    }
}