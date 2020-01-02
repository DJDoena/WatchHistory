namespace DoenaSoft.WatchHistory.YoutubeLink.Implementations
{
    using System;
    using System.Linq;
    using AbstractionLayer.IOServices;
    using MediaInfoHelper;
    using MediaInfoHelper.Youtube;
    using ToolBox.Extensions;
    using WatchHistory.Data;
    using WatchHistory.Implementations;

    internal class YoutubeFileManager
    {
        private readonly IDataManager _dataManager;

        private readonly IIOServices _ioServices;

        private readonly string _userName;

        public YoutubeFileManager(IDataManager dataManager
            , IIOServices ioServices
            , string userName)
        {
            _dataManager = dataManager;
            _ioServices = ioServices;
            _userName = userName;
        }

        public void Add(YoutubeVideoInfo info, DateTime watchedOn)
        {
            _dataManager.Suspend();

            try
            {
                TryProcess(info, watchedOn);
            }
            finally
            {
                _dataManager.Resume();
            }
        }

        private void TryProcess(YoutubeVideoInfo info, DateTime watchedOn)
        {
            var folder = _ioServices.Path.Combine(WatchHistory.Environment.AppDataFolder, "Youtube");

            if (_ioServices.Folder.Exists(folder) == false)
            {
                _ioServices.Folder.CreateFolder(folder);
            }

            _dataManager.RootFolders = folder.Enumerate().Union(_dataManager.RootFolders);

            _dataManager.FileExtensions = Constants.YoutubeFileExtensionName.Enumerate().Union(_dataManager.FileExtensions);

            CreateYoutubeFile(folder, info, watchedOn);
        }

        private void CreateYoutubeFile(string folder, YoutubeVideoInfo info, DateTime watchedOn)
        {
            var fileName = info.Id;

            _ioServices.Path.GetInvalidFileNameChars().ForEach(c => fileName.Replace(c, '_'));

            fileName = _ioServices.Path.Combine(folder, fileName + MediaInfoHelper.Constants.YoutubeFileExtension);

            SerializerHelper.Serialize(_ioServices, fileName, info);

            var fi = _ioServices.GetFileInfo(fileName);

            fi.CreationTime = info.Published;

            var fileEntry = new FileEntry()
            {
                FullName = fileName,
                Title = info.Title,
                CreationTime = fi.CreationTime.Conform(),
                VideoLength = info.RunningTime,
            };

            fileEntry = _dataManager.TryCreateEntry(fileEntry);

            _dataManager.AddWatched(fileEntry, _userName, watchedOn);
        }
    }
}