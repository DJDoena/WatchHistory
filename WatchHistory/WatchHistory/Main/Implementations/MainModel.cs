using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DoenaSoft.AbstractionLayer.IOServices;
using DoenaSoft.AbstractionLayer.UIServices;
using DoenaSoft.MediaInfoHelper.DataObjects;
using DoenaSoft.ToolBox.Extensions;
using DoenaSoft.WatchHistory.Data;
using DoenaSoft.WatchHistory.Implementations;
using DVDP = DoenaSoft.DVDProfiler.DVDProfilerXML.Version400;
using MIHC = DoenaSoft.MediaInfoHelper.Helpers.Constants;

namespace DoenaSoft.WatchHistory.Main.Implementations
{
    internal sealed class MainModel : ModelBase, IMainModel
    {
        private readonly IIOServices _ioServices;

        private readonly IUIServices _uiServices;

        private bool _ignoreWatched;

        public MainModel(IDataManager dataManager, IIOServices ioServices, IUIServices uiServices, string userName) : base(dataManager, userName)
        {
            _ioServices = ioServices;
            _uiServices = uiServices;
            this.IgnoreWatched = true;
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

                    this.RaiseFilesChanged(EventArgs.Empty);
                }
            }
        }

        public IEnumerable<FileEntry> GetFiles()
        {
            var allFiles = _dataManager.GetFiles().ToList();

            var notIgnoredFiles = allFiles.Except(allFiles.Where(this.UserIgnores));

            var filteredFiles = notIgnoredFiles.Where(this.ContainsFilter).ToList();

            var unwatchedFiles = this.IgnoreWatched
                ? filteredFiles.Except(filteredFiles.Where(this.UserHasWatched)).ToList()
                : filteredFiles;

            return unwatchedFiles;
        }

        public void ImportCollection()
        {
            var options = GetImportCollectionFileDialogOptions();

            if (_uiServices.ShowOpenFileDialog(options, out string fileName))
            {
                try
                {
                    var collection = SerializerHelper.Deserialize<DVDP.Collection>(_ioServices, fileName);

                    var processor = new CollectionProcessor(collection, _dataManager, _ioServices);

                    processor.Process();
                }
                catch
                {
                    _uiServices.ShowMessageBox("Collection file could not be read", string.Empty, Buttons.OK, Icon.Warning);
                }
            }

            this.RaiseFilesChanged(EventArgs.Empty);
        }

        public bool CanPlayFile(FileEntry entry) => _ioServices.GetFileInfo(entry.FullName).Exists && entry.FullName.EndsWith(MIHC.DvdProfilerFileExtension) == false;

        public void PlayFile(FileEntry entry)
        {
            if (this.CanPlayFile(entry))
            {
                if (entry.FullName.EndsWith(MIHC.YoutubeFileExtension))
                {
                    var info = SerializerHelper.Deserialize<YoutubeVideo>(_ioServices, entry.FullName);

                    var url = $"https://www.youtube.com/watch?v={info.Id}";

                    Process.Start(url);
                }
                else
                {
                    Process.Start(entry.FullName);
                }
            }
        }

        public bool CanOpenFileLocation(FileEntry entry) => _ioServices.GetFileInfo(entry.FullName).Exists;

        public void OpenFileLocation(FileEntry entry)
        {
            if (this.CanOpenFileLocation(entry))
            {
                Process.Start("explorer.exe", $"/select, \"{entry.FullName}\"");
            }
        }

        #endregion

        #region UserHasWatched

        private bool UserHasWatched(FileEntry file) => file.Users?.HasItemsWhere(this.UserHasWatched) == true;

        private bool UserHasWatched(Data.User user) => (this.IsUser(user)) && (user.Watches?.HasItems() == true);

        #endregion

        private static OpenFileDialogOptions GetImportCollectionFileDialogOptions()
        {
            var options = new OpenFileDialogOptions()
            {
                CheckFileExists = true,
                FileName = "Collection.xml",
                Filter = "Collection files|*.xml",
                RestoreFolder = true,
                Title = "Please select a DVD Profiler Collection Export file"
            };

            return options;
        }
    }
}