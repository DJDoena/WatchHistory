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

    internal sealed class MainModel : IMainModel
    {
        private readonly IDataManager _DataManager;

        private readonly IIOServices _IOServices;

        private readonly IUIServices _UIServices;

        private readonly String _UserName;

        private String _Filter;

        private Boolean _IgnoreWatched;

        private event EventHandler _FilesChanged;

        public MainModel(IDataManager dataManager
            , IIOServices ioServices
            , IUIServices uiServices
            , String userName)
        {
            _DataManager = dataManager;
            _IOServices = ioServices;
            _UIServices = uiServices;
            _UserName = userName;

            IgnoreWatched = true;
        }

        #region IMainModel

        public String Filter
        {
            get => _Filter ?? String.Empty;
            set
            {
                if (value != _Filter)
                {
                    _Filter = value;

                    RaiseFilesChanged(EventArgs.Empty);
                }
            }
        }

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

        public event EventHandler FilesChanged
        {
            add
            {
                if (_FilesChanged == null)
                {
                    _DataManager.FilesChanged += OnDataManagerFilesChanged;
                }

                _FilesChanged += value;
            }
            remove
            {
                _FilesChanged -= value;

                if (_FilesChanged == null)
                {
                    _DataManager.FilesChanged -= OnDataManagerFilesChanged;
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
            => fileEntry.FullName.EndsWith("." + Constants.DvdProfilerFileExtension) == false;

        public void OpenFileLocation(FileEntry fileEntry)
        {
            if (_IOServices.GetFileInfo(fileEntry.FullName).Exists)
            {
                Process.Start("explorer.exe", $"/select, \"{fileEntry.FullName}\"");
            }
        }

        #endregion

        #region UserIgnores

        private Boolean UserIgnores(FileEntry file)
            => file.Users?.HasItemsWhere(UserIgnores) == true;

        private Boolean UserIgnores(Data.User user)
            => (user.UserName == _UserName) && (user.Ignore);

        #endregion

        #region ContainsFilter

        private Boolean ContainsFilter(FileEntry file)
            => ContainsFilter(file, Filter.Trim().Split(' '));

        private static Boolean ContainsFilter(FileEntry file
           , IEnumerable<String> filters)
            => filters.All(filter => ContainsFilter(file, filter));

        private static Boolean ContainsFilter(FileEntry file
            , String filter)
            => file.FullName.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) != -1;

        #endregion

        #region UserHasWatched

        private Boolean UserHasWatched(FileEntry file)
            => file.Users?.HasItemsWhere(UserHasWatched) == true;

        private Boolean UserHasWatched(Data.User user)
            => (user.UserName == _UserName) && (user.Watches?.HasItems() == true);

        #endregion

        private static OpenFileDialogOptions GetImportCollectionFileDialogOptions()
        {
            OpenFileDialogOptions options = new OpenFileDialogOptions();

            options.CheckFileExists = true;
            options.FileName = "Collection.xml";
            options.Filter = "Collection files|*.xml";
            options.RestoreFolder = true;
            options.Title = "Please select a DVD Profiler Collection Export file";

            return (options);
        }

        private void OnDataManagerFilesChanged(Object sender
            , EventArgs e)
            => RaiseFilesChanged(e);

        private void RaiseFilesChanged(EventArgs e)
            => _FilesChanged?.Invoke(this, e);
    }
}