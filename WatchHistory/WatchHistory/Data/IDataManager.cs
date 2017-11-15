namespace DoenaSoft.WatchHistory.Data
{
    using System;
    using System.Collections.Generic;

    internal interface IDataManager
    {
        IEnumerable<String> Users { get; set; }

        IEnumerable<String> RootFolders { get; set; }

        IEnumerable<String> FileExtensions { get; set; }

        event EventHandler FilesChanged;

        IEnumerable<FileEntry> GetFiles();

        void AddWatched(FileEntry entry
            , String userName);

        void AddIgnore(FileEntry entry
            , String userName);

        void UndoIgnore(FileEntry entry
            , String userName);

        DateTime GetLastWatched(FileEntry entry
            , String userName);

        void SaveSettingsFile(String settingsFile);

        void SaveDataFile(String settingsFile);

        void Suspend();

        void Resume();

        DateTime GetCreationTime(FileEntry fileEntry);
    }
}