using System;
using System.Collections.Generic;
using DoenaSoft.WatchHistory.Data;

namespace DoenaSoft.WatchHistory.Ignore
{
    internal interface IIgnoreModel
    {
        String Filter { get;  set; }

        event EventHandler FilesChanged;

        IEnumerable<FileEntry> GetFiles();
    }
}