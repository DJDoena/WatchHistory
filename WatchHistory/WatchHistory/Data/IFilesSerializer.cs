namespace DoenaSoft.WatchHistory.Data
{
    internal interface IFilesSerializer
    {
        void CreateBackup(string fileName);

        Files LoadData(string fileName);

        void SaveFile(string fileName, Files files);
    }
}