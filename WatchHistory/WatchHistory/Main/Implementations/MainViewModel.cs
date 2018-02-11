namespace DoenaSoft.WatchHistory.Main.Implementations
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Input;
    using AbstractionLayer.IOServices;
    using Data;
    using ToolBox.Commands;
    using ToolBox.Extensions;
    using WatchHistory.Implementations;

    internal sealed class MainViewModel : IMainViewModel
    {
        private readonly IMainModel Model;

        private readonly IDataManager DataManager;

        private readonly IIOServices IOServices;

        private readonly IWindowFactory WindowFactory;

        private readonly String UserName;

        private event PropertyChangedEventHandler m_PropertyChanged;

        private SortColumn m_SortColumn;

        private Boolean SortAscending { get; set; }

        private Boolean SuspendEvents { get; set; }

        private Boolean EventRaisedWhileSuspended { get; set; }

        private SortColumn SortColumn
        {
            get
            {
                return (m_SortColumn);
            }
            set
            {
                SortAscending = (value != m_SortColumn) ? true : (SortAscending == false);

                m_SortColumn = value;

                RaisePropertyChanged(nameof(Entries));
            }
        }

        public MainViewModel(IMainModel model
            , IDataManager dataManager
            , IIOServices ioServices
            , IWindowFactory windowFactory
            , String userName)
        {
            Model = model;
            DataManager = dataManager;
            IOServices = ioServices;
            WindowFactory = windowFactory;
            UserName = userName;

            m_SortColumn = SortColumn.CreationTime;
            SortAscending = false;
            SuspendEvents = false;
            EventRaisedWhileSuspended = false;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (m_PropertyChanged == null)
                {
                    Model.FilesChanged += OnModelFilesChanged;
                }

                m_PropertyChanged += value;
            }
            remove
            {
                m_PropertyChanged -= value;

                if (m_PropertyChanged == null)
                {
                    Model.FilesChanged -= OnModelFilesChanged;
                }
            }
        }

        #endregion

        #region IMainViewModel

        public String Title
            => ($"Watch History (user: {UserName})");

        public String Filter
        {
            get
            {
                return (Model.Filter);
            }
            set
            {
                if (value != Model.Filter)
                {
                    Model.Filter = value;

                    RaisePropertyChanged(nameof(Filter));
                }
            }
        }

        public Boolean IgnoreWatched
        {
            get
            {
                return (Model.IgnoreWatched);
            }
            set
            {
                if (value != Model.IgnoreWatched)
                {
                    Model.IgnoreWatched = value;

                    RaisePropertyChanged(nameof(IgnoreWatched));
                }
            }
        }

        public ObservableCollection<IFileEntryViewModel> Entries
        {
            get
            {
                IEnumerable<FileEntry> modelEntries = Model.GetFiles();

                ObservableCollection<IFileEntryViewModel> viewModelEntries = ViewModelHelper.GetSortedEntries(modelEntries, UserName, DataManager, IOServices, SortColumn, SortAscending);

                return (viewModelEntries);
            }
        }

        public ICommand AddWatchedCommand
            => (new ParameterizedRelayCommand(AddWatched));

        public ICommand PlayFileAndAddWatchedCommand
            => (new ParameterizedRelayCommand(PlayFileAndAddWatched, CanPlayFile));

        public ICommand OpenSettingsCommand
            => (new RelayCommand(OpenSettings));

        public ICommand ImportCollectionCommand
            => (new RelayCommand(ImportCollection));

        public ICommand IgnoreCommand
           => (new ParameterizedRelayCommand(Ignore));

        public ICommand UndoIgnoreCommand
          => (new RelayCommand(UndoIgnore));

        public ICommand PlayFileCommand
            => (new ParameterizedRelayCommand(PlayFile, CanPlayFile));

        public ICommand SortCommand
            => (new ParameterizedRelayCommand(Sort));

        public ICommand OpenFileLocationCommand
            => (new ParameterizedRelayCommand(OpenFileLocation));

        #endregion

        private void AddWatched(Object parameter)
        {
            SuspendEvents = true;

            foreach (FileEntry fileEntry in GetEntries(parameter))
            {
                DataManager.AddWatched(fileEntry, UserName);
            }

            DataManager.SaveDataFile(WatchHistory.Environment.DataFile);

            ResumeEvents();
        }

        private void ResumeEvents()
        {
            SuspendEvents = false;

            if (EventRaisedWhileSuspended)
            {
                OnModelFilesChanged(this, EventArgs.Empty);

                EventRaisedWhileSuspended = false;
            }
        }

        private static IEnumerable<FileEntry> GetEntries(Object parameter)
            => (((IList)parameter).Cast<IFileEntryViewModel>().ForEach(entry => entry.FileEntry).ToList());

        private void Ignore(Object parameter)
        {
            SuspendEvents = true;

            foreach (FileEntry fileEntry in GetEntries(parameter))
            {
                DataManager.AddIgnore(fileEntry, UserName);
            }

            DataManager.SaveDataFile(WatchHistory.Environment.DataFile);

            ResumeEvents();
        }

        private void OpenSettings()
        {
            WindowFactory.OpenSettingsWindow();
        }

        private void UndoIgnore()
        {
            WindowFactory.OpenIgnoreWindow(UserName);
        }

        private void PlayFile(Object parameter)
        {
            FileEntry fileEntry = GetFileEntry(parameter);

            Model.PlayFile(fileEntry);
        }

        private Boolean CanPlayFile(Object parameter)
        {
            FileEntry fileEntry = GetFileEntry(parameter);

            Boolean canPlay = (fileEntry != null) && (Model.CanPlayFile(fileEntry));

            return (canPlay);
        }

        private void PlayFileAndAddWatched(Object parameter)
        {
            FileEntry fileEntry = GetFileEntry(parameter);

            Model.PlayFile(fileEntry);

            DataManager.AddWatched(fileEntry, UserName);

            DataManager.SaveDataFile(WatchHistory.Environment.DataFile);
        }

        private static FileEntry GetFileEntry(Object parameter)
            => (((IFileEntryViewModel)parameter)?.FileEntry);

        private void OpenFileLocation(Object parameter)
        {
            FileEntry fileEntry = GetFileEntry(parameter);

            Model.OpenFileLocation(fileEntry);
        }

        private void Sort(Object parameter)
        {
            SortColumn = (SortColumn)(Enum.Parse(typeof(SortColumn), (String)parameter));
        }

        private void ImportCollection()
        {
            SuspendEvents = true;

            Model.ImportCollection();

            ResumeEvents();
        }

        private void OnModelFilesChanged(Object sender
            , EventArgs e)
        {
            if (SuspendEvents == false)
            {
                RaisePropertyChanged(nameof(Entries));
            }
            else
            {
                EventRaisedWhileSuspended = true;
            }
        }

        private void RaisePropertyChanged(String attribute)
        {
            m_PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
        }
    }
}