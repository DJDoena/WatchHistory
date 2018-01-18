namespace DoenaSoft.WatchHistory.Main.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using AbstractionLayer.IOServices;
    using AbstractionLayer.UIServices;
    using Data;
    using DVDProfiler.DVDProfilerHelper;
    using DVDProfiler.DVDProfilerXML.Version390;
    using ToolBox.Extensions;

    internal sealed class MainModel : IMainModel
    {
        private readonly IDataManager DataManager;

        private readonly IIOServices IOServices;

        private readonly IUIServices UIServices;

        private readonly String UserName;

        private String m_Filter;

        private Boolean m_IgnoreWatched;

        private event EventHandler m_FilesChanged;

        public MainModel(IDataManager dataManager
            , IIOServices ioServices
            , IUIServices uiServices
            , String userName)
        {
            DataManager = dataManager;
            IOServices = ioServices;
            UIServices = uiServices;
            UserName = userName;

            IgnoreWatched = true;
        }

        #region IMainModel

        public String Filter
        {
            get
            {
                return (m_Filter ?? String.Empty);
            }
            set
            {
                if (value != m_Filter)
                {
                    m_Filter = value;

                    RaiseFilesChanged(EventArgs.Empty);
                }
            }
        }

        public Boolean IgnoreWatched
        {
            get
            {
                return (m_IgnoreWatched);
            }
            set
            {
                if (value != m_IgnoreWatched)
                {
                    m_IgnoreWatched = value;

                    RaiseFilesChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler FilesChanged
        {
            add
            {
                if (m_FilesChanged == null)
                {
                    DataManager.FilesChanged += OnDataManagerFilesChanged;
                }

                m_FilesChanged += value;
            }
            remove
            {
                m_FilesChanged -= value;

                if (m_FilesChanged == null)
                {
                    DataManager.FilesChanged -= OnDataManagerFilesChanged;
                }
            }
        }

        public IEnumerable<FileEntry> GetFiles()
        {
            IEnumerable<FileEntry> files = DataManager.GetFiles();

            files = files.Except(files.Where(UserIgnores));

            files = files.Where(ContainsFilter);

            if (IgnoreWatched)
            {
                files = files.Except(files.Where(UserHasWatched));
            }

            files = files.ToList();

            return (files);
        }

        public void ImportCollection()
        {
            OpenFileDialogOptions options = GetImportCollectionFileDialogOptions();

            String fileName;
            if (UIServices.ShowOpenFileDialog(options, out fileName))
            {
                try
                {
                    Collection collection = Serializer<Collection>.Deserialize(fileName);

                    CollectionProcessor processor = new CollectionProcessor(collection, DataManager, IOServices);

                    processor.Process();
                }
                catch
                {
                    UIServices.ShowMessageBox("Collection file could not be read", String.Empty, Buttons.OK, Icon.Warning);
                }
            }

            RaiseFilesChanged(EventArgs.Empty);
        }

        public void PlayFile(FileEntry fileEntry)
        {
            if (IOServices.GetFileInfo(fileEntry.FullName).Exists)
            {
                Process.Start(fileEntry.FullName);
            }
        }

        public Boolean CanPlayFile(FileEntry fileEntry)
            => (fileEntry.FullName.EndsWith("." + CollectionProcessor.FileExtension) == false);

        public void OpenFileLocation(FileEntry fileEntry)
        {
            if (IOServices.GetFileInfo(fileEntry.FullName).Exists)
            {
                Process.Start("explorer.exe", $"/select, \"{fileEntry.FullName}\"");
            }
        }

        #endregion

        #region UserIgnores

        private Boolean UserIgnores(FileEntry file)
            => (file.Users?.HasItemsWhere(UserIgnores) == true);

        private Boolean UserIgnores(Data.User user)
            => ((user.UserName == UserName) && (user.Ignore));

        #endregion

        #region ContainsFilter

        private Boolean ContainsFilter(FileEntry file)
            => (ContainsFilter(file, Filter.Trim().Split(' ')));

        private static Boolean ContainsFilter(FileEntry file
           , IEnumerable<String> filters)
            => (filters.All(filter => ContainsFilter(file, filter)));

        private static Boolean ContainsFilter(FileEntry file
            , String filter)
            => (file.FullName.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) != -1);

        #endregion

        #region UserHasWatched

        private Boolean UserHasWatched(FileEntry file)
            => (file.Users?.HasItemsWhere(UserHasWatched) == true);

        private Boolean UserHasWatched(Data.User user)
            => ((user.UserName == UserName) && (user.Watches?.HasItems() == true));

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
        {
            RaiseFilesChanged(e);
        }

        private void RaiseFilesChanged(EventArgs e)
        {
            m_FilesChanged?.Invoke(this, e);
        }
    }
}