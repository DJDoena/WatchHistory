namespace DoenaSoft.WatchHistory.Main
{
    using System;
    using System.Collections.Generic;
    using Data;

    internal interface IMainModel
    {
        string Filter { get; set; }

        bool IgnoreWatched { get; set; }

        bool SearchInPath { get; set; }

        event EventHandler FilesChanged;

        IEnumerable<FileEntry> GetFiles();

        void ImportCollection();

        bool CanPlayFile(FileEntry entry);

        void PlayFile(FileEntry entry);

        bool CanOpenFileLocation(FileEntry entry);

        void OpenFileLocation(FileEntry entry);
    }
}