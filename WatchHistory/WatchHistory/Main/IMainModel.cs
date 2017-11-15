namespace DoenaSoft.WatchHistory.Main
{
    using System;
    using System.Collections.Generic;
    using Data;

    internal interface IMainModel
    {
        String Filter { get; set; }

        Boolean IgnoreWatched { get; set; }

        event EventHandler FilesChanged;

        IEnumerable<FileEntry> GetFiles();

        void ImportCollection();

        void PlayFile(FileEntry fileEntry);

        void OpenFileLocation(FileEntry fileEntry);
    }
}