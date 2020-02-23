namespace DoenaSoft.WatchHistory.IgnoreEntry
{
    using System;
    using System.Collections.Generic;
    using Data;

    internal interface IIgnoreEntryModel
    {
        string Filter { get; set; }

        bool SearchInPath { get; set; }

        event EventHandler FilesChanged;

        IEnumerable<FileEntry> GetFiles();
    }
}