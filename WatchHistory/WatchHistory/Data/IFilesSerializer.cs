namespace DoenaSoft.WatchHistory.Data
{
    internal interface IFilesSerializer
    {
        void CreateBackup(string file);

        Files LoadData(string file);

        void SaveFile(string file, Files files);
    }
}