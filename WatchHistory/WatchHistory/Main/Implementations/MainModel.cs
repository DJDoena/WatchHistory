namespace DoenaSoft.WatchHistory.Main.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using AbstractionLayer.IOServices;
    using AbstractionLayer.UIServices;
    using Data;
    using DVDProfiler.DVDProfilerXML.Version400;
    using MediaInfoHelper.Youtube;
    using ToolBox.Extensions;
    using WatchHistory.Implementations;

    internal sealed class MainModel : ModelBase, IMainModel
    {
        private readonly IIOServices _ioServices;

        private readonly IUIServices _uiServices;

        private bool _ignoreWatched;

        public MainModel(IDataManager dataManager
            , IIOServices ioServices
            , IUIServices uiServices
            , string userName)
            : base(dataManager, userName)
        {
            _ioServices = ioServices;
            _uiServices = uiServices;
            IgnoreWatched = true;
        }

        #region IMainModel

        public bool IgnoreWatched
        {
            get => _ignoreWatched;
            set
            {
                if (value != _ignoreWatched)
                {
                    _ignoreWatched = value;

                    RaiseFilesChanged(EventArgs.Empty);
                }
            }
        }

        public IEnumerable<FileEntry> GetFiles()
        {
            var allFiles = _dataManager.GetFiles();

            var notIgnoredFiles = allFiles.Except(allFiles.Where(UserIgnores));

            var filteredFiles = notIgnoredFiles.Where(ContainsFilter);

            var unwatchedFiles = IgnoreWatched ? filteredFiles.Except(filteredFiles.Where(UserHasWatched)) : filteredFiles;

            var result = unwatchedFiles.ToList();

            return result;
        }

        public void ImportCollection()
        {
            OpenFileDialogOptions options = GetImportCollectionFileDialogOptions();

            if (_uiServices.ShowOpenFileDialog(options, out string fileName))
            {
                try
                {
                    Collection collection = SerializerHelper.Deserialize<Collection>(_ioServices, fileName);

                    CollectionProcessor processor = new CollectionProcessor(collection, _dataManager, _ioServices);

                    processor.Process();
                }
                catch
                {
                    _uiServices.ShowMessageBox("Collection file could not be read", string.Empty, Buttons.OK, Icon.Warning);
                }
            }

            RaiseFilesChanged(EventArgs.Empty);
        }

        public void PlayFile(FileEntry fileEntry)
        {
            if (fileEntry.FullName.EndsWith(MediaInfoHelper.Constants.YoutubeFileExtension))
            {
                var info = SerializerHelper.Deserialize<YoutubeVideoInfo>(_ioServices, fileEntry.FullName);

                var url = $"https://www.youtube.com/watch?v={info.Id}";

                Process.Start(url);
            }
            else
            {
                Process.Start(fileEntry.FullName);
            }

        }

        public bool CanPlayFile(FileEntry fileEntry)
            => _ioServices.GetFileInfo(fileEntry.FullName).Exists && fileEntry.FullName.EndsWith(MediaInfoHelper.Constants.DvdProfilerFileExtension) == false;

        public void OpenFileLocation(FileEntry fileEntry)
        {
            if (_ioServices.GetFileInfo(fileEntry.FullName).Exists)
            {
                Process.Start("explorer.exe", $"/select, \"{fileEntry.FullName}\"");
            }
        }

        public IEnumerable<Watch> GetWatches(FileEntry fileEntry)
            => fileEntry.Users?.FirstOrDefault(IsUser)?.Watches?.ToList() ?? Enumerable.Empty<Watch>();

        #endregion

        #region UserHasWatched

        private bool UserHasWatched(FileEntry file)
            => file.Users?.HasItemsWhere(UserHasWatched) == true;

        private bool UserHasWatched(Data.User user)
            => (IsUser(user)) && (user.Watches?.HasItems() == true);

        #endregion

        private static OpenFileDialogOptions GetImportCollectionFileDialogOptions()
        {
            OpenFileDialogOptions options = new OpenFileDialogOptions()
            {
                CheckFileExists = true,
                FileName = "Collection.xml",
                Filter = "Collection files|*.xml",
                RestoreFolder = true,
                Title = "Please select a DVD Profiler Collection Export file"
            };

            return (options);
        }
    }
}