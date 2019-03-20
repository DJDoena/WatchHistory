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
    using ToolBox.Extensions;
    using WatchHistory.Implementations;

    internal sealed class MainModel : ModelBase, IMainModel
    {
        private readonly IIOServices _ioServices;

        private readonly IUIServices _uiServices;

        private Boolean _ignoreWatched;
        
        public MainModel(IDataManager dataManager
            , IIOServices ioServices
            , IUIServices uiServices
            , String userName)
            : base(dataManager, userName)
        {
            _ioServices = ioServices;
            _uiServices = uiServices;
            IgnoreWatched = true;
        }

        #region IMainModel

        public Boolean IgnoreWatched
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
            var allFiles = _DataManager.GetFiles();

            var notIgnoredFiles = allFiles.Except(allFiles.Where(UserIgnores));

            var filteredFiles = notIgnoredFiles.Where(ContainsFilter);

            var unwatchedFiles = IgnoreWatched ? filteredFiles.Except(filteredFiles.Where(UserHasWatched)) : filteredFiles;

            var result = unwatchedFiles.ToList();

            return result;
        }

        public void ImportCollection()
        {
            OpenFileDialogOptions options = GetImportCollectionFileDialogOptions();

            if (_uiServices.ShowOpenFileDialog(options, out String fileName))
            {
                try
                {
                    Collection collection = SerializerHelper.Deserialize<Collection>(_ioServices, fileName);

                    CollectionProcessor processor = new CollectionProcessor(collection, _DataManager, _ioServices);

                    processor.Process();
                }
                catch
                {
                    _uiServices.ShowMessageBox("Collection file could not be read", String.Empty, Buttons.OK, Icon.Warning);
                }
            }

            RaiseFilesChanged(EventArgs.Empty);
        }

        public void PlayFile(FileEntry fileEntry)
        {
            if (_ioServices.GetFileInfo(fileEntry.FullName).Exists)
            {
                Process.Start(fileEntry.FullName);
            }
        }

        public Boolean CanPlayFile(FileEntry fileEntry)
            => fileEntry.FullName.EndsWith(Constants.DvdProfilerFileExtension) == false;

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

        private Boolean UserHasWatched(FileEntry file)
            => file.Users?.HasItemsWhere(UserHasWatched) == true;

        private Boolean UserHasWatched(Data.User user)
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