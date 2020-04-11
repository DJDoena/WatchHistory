using System.Collections.Generic;

namespace DoenaSoft.WatchHistory.Data
{
    internal interface IFilesSerializer
    {
        void CreateBackup(string fileName);

        IEnumerable<FileEntry> LoadData(string fileName);

        void SaveData(string fileName, IEnumerable<FileEntry> entries);

        DefaultValues LoadSettings(string fileName);

        void SaveSettings(string fileName, DefaultValues defaultValues);
    }
}