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
        private readonly IIOServices _IOServices;

        private readonly IUIServices _UIServices;

        private Boolean _IgnoreWatched;

        public MainModel(IDataManager dataManager
            , IIOServices ioServices
            , IUIServices uiServices
            , String userName)
            : base(dataManager, userName)
        {
            _IOServices = ioServices;
            _UIServices = uiServices;
            IgnoreWatched = true;
        }

        #region IMainModel

        public Boolean IgnoreWatched
        {
            get => _IgnoreWatched;
            set
            {
                if (value != _IgnoreWatched)
                {
                    _IgnoreWatched = value;

                    RaiseFilesChanged(EventArgs.Empty);
                }
            }
        }

        public IEnumerable<FileEntry> GetFiles()
        {
            IEnumerable<FileEntry> allFiles = _DataManager.GetFiles();

            IEnumerable<FileEntry> notIgnoredFiles = allFiles.Except(allFiles.Where(UserIgnores));

            IEnumerable<FileEntry> filteredFiles = notIgnoredFiles.Where(ContainsFilter).ToList();

            IEnumerable<FileEntry> unwatchedFiles = filteredFiles.Except(filteredFiles.Where(UserHasWatched));

            IEnumerable<FileEntry> result = IgnoreWatched ? unwatchedFiles.ToList() : filteredFiles;

            return (result);
        }

        public void ImportCollection()
        {
            OpenFileDialogOptions options = GetImportCollectionFileDialogOptions();

            if (_UIServices.ShowOpenFileDialog(options, out String fileName))
            {
                try
                {
                    Collection collection = SerializerHelper.Deserialize<Collection>(_IOServices, fileName);

                    CollectionProcessor processor = new CollectionProcessor(collection, _DataManager, _IOServices);

                    processor.Process();
                }
                catch
                {
                    _UIServices.ShowMessageBox("Collection file could not be read", String.Empty, Buttons.OK, Icon.Warning);
                }
            }

            RaiseFilesChanged(EventArgs.Empty);
        }

        public void PlayFile(FileEntry fileEntry)
        {
            if (_IOServices.GetFileInfo(fileEntry.FullName).Exists)
            {
                Process.Start(fileEntry.FullName);
            }
        }

        public Boolean CanPlayFile(FileEntry fileEntry)
            => fileEntry.FullName.EndsWith(Constants.DvdProfilerFileExtension) == false;

        public void OpenFileLocation(FileEntry fileEntry)
        {
            if (_IOServices.GetFileInfo(fileEntry.FullName).Exists)
            {
                Process.Start("explorer.exe", $"/select, \"{fileEntry.FullName}\"");
            }
        }

        public IEnumerable<Watch> GetWatches(FileEntry fileEntry)
            => fileEntry.Users?.FirstOrDefault(IsUser)?.Watches.ToList() ?? Enumerable.Empty<Watch>();

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