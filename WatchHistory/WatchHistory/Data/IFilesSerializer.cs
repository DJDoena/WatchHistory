namespace DoenaSoft.WatchHistory.Data
{
    using System;

    internal interface IFilesSerializer
    {
        void CreateBackup(String fileName);

        Files LoadData(String fileName);

        void SaveFile(String fileName
            , Files files);
    }
}