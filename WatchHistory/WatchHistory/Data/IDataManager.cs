using System;
using System.Collections.Generic;
using DoenaSoft.MediaInfoHelper.DataObjects;

namespace DoenaSoft.WatchHistory.Data
{
    internal interface IDataManager
    {
        IEnumerable<string> Users { get; set; }

        IEnumerable<string> RootFolders { get; set; }

        IEnumerable<string> FileExtensions { get; set; }

        bool IsSynchronizing { get; }

        event EventHandler IsSynchronizingChanged;

        event EventHandler FilesChanged;

        IEnumerable<FileEntry> GetFiles();

        void AddWatched(FileEntry entry, string userName);

        void AddWatched(FileEntry entry, string userName, DateTime watchedOn);

        void AddIgnore(FileEntry entry, string userName);

        void UndoIgnore(FileEntry entry, string userName);

        DateTime GetLastWatched(FileEntry entry, string userName);

        void SaveSettingsFile();

        void SaveDataFile();

        void Suspend();

        void Resume();

        DateTime GetCreationTime(FileEntry entry);

        MediaFile DetermineVideoLength(FileEntry entry);

        FileEntry TryCreateEntry(FileEntry entry);
    }
}