namespace DoenaSoft.WatchHistory.Ignore
{
    using System;
    using System.Collections.Generic;
    using Data;

    internal interface IIgnoreModel
    {
        string Filter { get; set; }

        bool SearchInPath { get; set; }

        event EventHandler FilesChanged;

        IEnumerable<FileEntry> GetFiles();
    }
}