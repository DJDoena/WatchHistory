using System;
using System.Linq;
using DoenaSoft.AbstractionLayer.IOServices;
using DoenaSoft.MediaInfoHelper.DataObjects;
using DoenaSoft.MediaInfoHelper.Helpers;
using DoenaSoft.ToolBox.Extensions;
using DoenaSoft.WatchHistory.Data;
using DoenaSoft.WatchHistory.Implementations;
using MIHC = DoenaSoft.MediaInfoHelper.Helpers.Constants;

namespace DoenaSoft.WatchHistory.AddYoutubeLink.Implementations
{
    internal class YoutubeFileManager
    {
        private readonly IDataManager _dataManager;

        private readonly IIOServices _ioServices;

        private readonly string _userName;

        public YoutubeFileManager(IDataManager dataManager, IIOServices ioServices, string userName)
        {
            _dataManager = dataManager;
            _ioServices = ioServices;
            _userName = userName;
        }

        public void Add(YoutubeVideo info, DateTime watchedOn)
        {
            _dataManager.Suspend();

            try
            {
                this.TryProcess(info, watchedOn);
            }
            finally
            {
                _dataManager.Resume();
            }
        }

        private void TryProcess(YoutubeVideo info, DateTime watchedOn)
        {
            var folder = _ioServices.Path.Combine(WatchHistory.Environment.MyDocumentsFolder, "Youtube");

            if (_ioServices.Folder.Exists(folder) == false)
            {
                _ioServices.Folder.CreateFolder(folder);
            }

            _dataManager.RootFolders = folder.Enumerate().Union(_dataManager.RootFolders);

            _dataManager.FileExtensions = MIHC.YoutubeFileExtensionName.Enumerate().Union(_dataManager.FileExtensions);

            this.CreateYoutubeFile(folder, info, watchedOn);
        }

        private void CreateYoutubeFile(string folder, YoutubeVideo info, DateTime watchedOn)
        {
            var fileName = info.Id;

            _ioServices.Path.GetInvalidFileNameChars().ForEach(c => fileName.Replace(c, '_'));

            fileName = _ioServices.Path.Combine(folder, fileName + MIHC.YoutubeFileExtension);

            SerializerHelper.Serialize(_ioServices, fileName, info);

            var fi = _ioServices.GetFile(fileName);

            fi.CreationTimeUtc = info.Published.ToUniversalTime().Conform();

            var fileEntry = new FileEntry()
            {
                FullName = fileName,
                Title = info.Title,
                CreationTime = fi.CreationTimeUtc,
                VideoLength = info.RunningTime,
            };

            fileEntry = _dataManager.TryCreateEntry(fileEntry);

            _dataManager.AddWatched(fileEntry, _userName, watchedOn);
        }
    }
}