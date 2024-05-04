using DoenaSoft.AbstractionLayer.IOServices;
using DoenaSoft.WatchHistory.Implementations;

namespace DoenaSoft.WatchHistory.Data.Implementations
{
    internal sealed class DataUpgrader
    {
        private readonly IIOServices _ioServices;

        private string _appDataFolder;

        private string _appDataFolderWithoutDot;

        private string _myDocumentsFolder;

        public DataUpgrader(IIOServices ioServices)
        {
            _ioServices = ioServices;
        }

        public void Upgrade()
        {
            var appDataFolder = _ioServices.Folder.GetFullPath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData));

            _appDataFolder = _ioServices.Path.Combine(appDataFolder, "Doena Soft.", "WatchHistory");

            if (_ioServices.Folder.Exists(_appDataFolder))
            {
                this.Upgrade(_appDataFolder);
            }
        }

        private void Upgrade(string appDataFolder)
        {
            var myDocumentsFolder = _ioServices.Folder.GetFullPath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments));

            _myDocumentsFolder = _ioServices.Path.Combine(myDocumentsFolder, "WatchHistory");

            if (_ioServices.Folder.Exists(_myDocumentsFolder))
            {
                return;
            }

            var appDataFolderWithoutDot = _ioServices.Folder.GetFullPath(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData));

            _appDataFolderWithoutDot = _ioServices.Path.Combine(appDataFolderWithoutDot, "Doena Soft", "WatchHistory");

            this.MoveFiles();

            this.UpdatePaths();

            _ioServices.Folder.Delete(appDataFolder);
        }

        private void MoveFiles()
        {
            _ioServices.Folder.CreateFolder(_myDocumentsFolder);

            var oldFiles = _ioServices.Folder.GetFiles(_appDataFolder, searchOption: System.IO.SearchOption.AllDirectories);

            foreach (var oldFile in oldFiles)
            {
                var newFileName = oldFile.FullName.Replace(_appDataFolder, _myDocumentsFolder).Replace(_appDataFolderWithoutDot, _myDocumentsFolder);

                var newFile = _ioServices.GetFileInfo(newFileName);

                if (!newFile.Folder.Exists)
                {
                    newFile.Folder.Create();
                }

                _ioServices.File.Copy(oldFile.FullName, newFile.FullName, false);

                newFile.CreationTime = oldFile.CreationTime;
                newFile.LastWriteTime = oldFile.CreationTime;

                _ioServices.File.Delete(oldFile.FullName);
            }
        }

        private void UpdatePaths()
        {
            this.UpdateDataFilePaths();

            this.UpdateSettingFilePaths();
        }

        private void UpdateDataFilePaths()
        {
            var dataFile = _ioServices.Path.Combine(_myDocumentsFolder, "Files.xml");

            var files = SerializerHelper.Deserialize<Files>(_ioServices, dataFile);

            if (files?.Entries?.Length > 0)
            {
                this.UpdatePaths(files.Entries);

                SerializerHelper.Serialize(_ioServices, dataFile, files);
            }
        }

        private void UpdatePaths(FileEntry[] entries)
        {
            foreach (var entry in entries)
            {
                entry.FullName = entry.FullName?.Replace(_appDataFolder, _myDocumentsFolder).Replace(_appDataFolderWithoutDot, _myDocumentsFolder);
            }
        }

        private void UpdateSettingFilePaths()
        {
            var settingFile = _ioServices.Path.Combine(_myDocumentsFolder, "Settings.xml");

            var settings = SerializerHelper.Deserialize<Settings>(_ioServices, settingFile);

            if (settings?.DefaultValues?.RootFolders?.Length > 0)
            {
                this.UpdatePaths(settings.DefaultValues.RootFolders);

                SerializerHelper.Serialize(_ioServices, settingFile, settings);
            }
        }

        private void UpdatePaths(string[] rootFolders)
        {
            for (var folderIndex = 0; folderIndex < rootFolders.Length; folderIndex++)
            {
                rootFolders[folderIndex] = rootFolders[folderIndex]?.Replace(_appDataFolder, _myDocumentsFolder).Replace(_appDataFolderWithoutDot, _myDocumentsFolder);
            }
        }
    }
}