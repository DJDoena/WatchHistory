namespace DoenaSoft.WatchHistory.Ignore
{
    using System;
    using System.Collections.Generic;
    using Data;

    internal interface IIgnoreModel
    {
        String Filter { get; set; }

        Boolean SearchInPath { get; set; }

        event EventHandler FilesChanged;

        IEnumerable<FileEntry> GetFiles();
    }
}