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

        bool CanPlayFile(FileEntry fileEntry);

        void PlayFile(FileEntry fileEntry);

        bool CanOpenFileLocation(FileEntry fileEntry);

        void OpenFileLocation(FileEntry fileEntry);

        IEnumerable<Watch> GetWatches(FileEntry fileEntry);
    }
}