using System;
using System.Collections.Generic;
using DoenaSoft.WatchHistory.Data;

namespace DoenaSoft.WatchHistory.Main
{
    internal interface IMainModel
    {
        String Filter { get;  set; }

        Boolean IgnoreWatched { get; set; }

        event EventHandler FilesChanged;

        IEnumerable<FileEntry> GetFiles();

        void ImportCollection();
    }
}