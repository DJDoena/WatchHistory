namespace DoenaSoft.WatchHistory.Data
{
    using System;
    using System.Collections.Generic;

    internal interface IDataManager
    {
        IEnumerable<String> Users { get; set; }

        IEnumerable<String> RootFolders { get; set; }

        IEnumerable<String> FileExtensions { get; set; }

        Boolean IsSynchronizing { get; }

        event EventHandler IsSynchronizingChanged;

        event EventHandler FilesChanged;

        IEnumerable<FileEntry> GetFiles();

        void AddWatched(FileEntry entry
            , String userName);

        void AddWatched(FileEntry entry
            , String userName
            , DateTime watchedOn);

        void AddIgnore(FileEntry entry
            , String userName);

        void UndoIgnore(FileEntry entry
            , String userName);

        DateTime GetLastWatched(FileEntry entry
            , String userName);

        void SaveSettingsFile(String fileName);

        void SaveDataFile(String fileName);

        void Suspend();

        void Resume();

        DateTime GetCreationTime(FileEntry fileEntry);
    }
}