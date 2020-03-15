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
    using DVDProfiler.DVDProfilerHelper;
    using ToolBox.Commands;
    using ToolBox.Extensions;
    using WatchHistory.Implementations;

    internal sealed class MainViewModel : IMainViewModel
    {
        private readonly IMainModel _model;

        private readonly IDataManager _dataManager;

        private readonly IIOServices _ioServices;

        private readonly IWindowFactory _windowFactory;

        private readonly string _userName;

#pragma warning disable IDE1006 // Naming Styles
        private event PropertyChangedEventHandler _propertyChanged;
#pragma warning restore IDE1006 // Naming Styles

        private SortColumn _sortColumn;

        private bool SortAscending { get; set; }

        private bool SuspendEvents { get; set; }

        private bool EventRaisedWhileSuspended { get; set; }

        private SortColumn SortColumn
        {
            get => _sortColumn;
            set
            {
                SortAscending = (value != _sortColumn) ? true : (SortAscending == false);

                _sortColumn = value;

                RaisePropertyChanged(nameof(Entries));
            }
        }

        public MainViewModel(IMainModel model, IDataManager dataManager, IIOServices ioServices, IWindowFactory windowFactory, string userName)
        {
            _model = model;
            _dataManager = dataManager;
            _ioServices = ioServices;
            _windowFactory = windowFactory;
            _userName = userName;

            _sortColumn = SortColumn.CreationTime;
            SortAscending = false;
            SuspendEvents = false;
            EventRaisedWhileSuspended = false;

            AddWatchedCommand = new ParameterizedRelayCommand(AddWatched);
            PlayFileAndAddWatchedCommand = new ParameterizedRelayCommand(PlayFileAndAddWatched, CanPlayFile);
            OpenSettingsCommand = new RelayCommand(OpenSettings);
            ImportCollectionCommand = new RelayCommand(ImportCollection, CanImportCollection);
            IgnoreCommand = new ParameterizedRelayCommand(Ignore);
            UndoIgnoreCommand = new RelayCommand(UndoIgnore);
            PlayFileCommand = new ParameterizedRelayCommand(PlayFile, CanPlayFile);
            SortCommand = new ParameterizedRelayCommand(Sort);
            OpenFileLocationCommand = new ParameterizedRelayCommand(OpenFileLocation, CanOpenFileLocation);
            AddWatchedOnCommand = new ParameterizedRelayCommand(AddWatchedOn);
            CheckForUpdateCommand = new RelayCommand(CheckForUpdate);
            AboutCommand = new RelayCommand(ShowAbout);
            ShowHistoryCommand = new ParameterizedRelayCommand(ShowHistory);
            EditRunningTimeCommand = new ParameterizedRelayCommand(EditRunningTime);
            AddYoutubeLinkCommand = new RelayCommand(AddYoutubeLink, CanAddYoutubeLink);
            AddManualEntryCommand = new RelayCommand(AddAddManualEntry, CanAddManualEntry);
            EditTitleCommand = new ParameterizedRelayCommand(EditTitle);
            ShowReportCommand = new RelayCommand(ShowReport);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (_propertyChanged == null)
                {
                    _model.FilesChanged += OnModelFilesChanged;

                    _dataManager.IsSynchronizingChanged += OnDataManagerIsSynchronizingChanged;
                }

                _propertyChanged += value;
            }
            remove
            {
                _propertyChanged -= value;

                if (_propertyChanged == null)
                {
                    _dataManager.IsSynchronizingChanged -= OnDataManagerIsSynchronizingChanged;

                    _model.FilesChanged -= OnModelFilesChanged;
                }
            }
        }

        #endregion

        #region IMainViewModel

        public string Title => $"Watch History (user: {_userName})";

        public string Filter
        {
            get => _model.Filter;
            set
            {
                if (value != _model.Filter)
                {
                    _model.Filter = value;

                    RaisePropertyChanged(nameof(Filter));
                }
            }
        }

        public bool IgnoreWatched
        {
            get => _model.IgnoreWatched;
            set
            {
                if (value != _model.IgnoreWatched)
                {
                    _model.IgnoreWatched = value;

                    RaisePropertyChanged(nameof(IgnoreWatched));
                }
            }
        }

        public bool SearchInPath
        {
            get => _model.SearchInPath;
            set
            {
                if (value != _model.SearchInPath)
                {
                    _model.SearchInPath = value;

                    RaisePropertyChanged(nameof(SearchInPath));
                }
            }
        }

        public ObservableCollection<IFileEntryViewModel> Entries
        {
            get
            {
                var modelEntries = _model.GetFiles();

                var viewModelEntries = ViewModelHelper.GetSortedEntries(modelEntries, _userName, _dataManager, _ioServices, SortColumn, SortAscending);

                return viewModelEntries;
            }
        }

        public ICommand AddWatchedCommand { get; }

        public ICommand PlayFileAndAddWatchedCommand { get; }

        public ICommand OpenSettingsCommand { get; }

        public ICommand ImportCollectionCommand { get; }

        public ICommand IgnoreCommand { get; }

        public ICommand UndoIgnoreCommand { get; }

        public ICommand PlayFileCommand { get; }

        public ICommand SortCommand { get; }

        public ICommand OpenFileLocationCommand { get; }

        public ICommand AddWatchedOnCommand { get; }

        public ICommand CheckForUpdateCommand { get; }

        public ICommand AboutCommand { get; }

        public ICommand ShowHistoryCommand { get; }

        public ICommand EditRunningTimeCommand { get; }

        public ICommand AddYoutubeLinkCommand { get; }

        public ICommand AddManualEntryCommand { get; }

        public ICommand EditTitleCommand { get; }

        public ICommand ShowReportCommand { get; }

        #endregion

        private void AddWatched(object parameter)
        {
            SuspendEvents = true;

            GetEntries(parameter).ForEach(entry => _dataManager.AddWatched(entry, _userName));

            _dataManager.SaveDataFile();

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

        private static IEnumerable<FileEntry> GetEntries(object parameter) => ((IList)parameter).Cast<IFileEntryViewModel>().Select(entry => entry.Entry).ToList();

        private void Ignore(object parameter)
        {
            SuspendEvents = true;

            GetEntries(parameter).ForEach(entry => _dataManager.AddIgnore(entry, _userName));

            _dataManager.SaveDataFile();

            ResumeEvents();
        }

        private void OpenSettings() => _windowFactory.OpenSettingsWindow();

        private void UndoIgnore() => _windowFactory.OpenIgnoreWindow(_userName, Filter);

        private bool CanPlayFile(object parameter)
        {
            var fileEntry = GetFileEntry(parameter);

            var canPlay = (fileEntry != null) && (_model.CanPlayFile(fileEntry));

            return canPlay;
        }

        private void PlayFile(object parameter)
        {
            var fileEntry = GetFileEntry(parameter);

            _model.PlayFile(fileEntry);
        }

        private void PlayFileAndAddWatched(object parameter)
        {
            var fileEntry = GetFileEntry(parameter);

            _model.PlayFile(fileEntry);

            _dataManager.AddWatched(fileEntry, _userName);

            _dataManager.SaveDataFile();
        }

        private static FileEntry GetFileEntry(object parameter) => ((IFileEntryViewModel)parameter)?.Entry;

        private bool CanOpenFileLocation(object parameter)
        {
            var fileEntry = GetFileEntry(parameter);

            var canOpen = (fileEntry != null) && (_model.CanOpenFileLocation(fileEntry));

            return canOpen;
        }

        private void OpenFileLocation(object parameter)
        {
            var fileEntry = GetFileEntry(parameter);

            _model.OpenFileLocation(fileEntry);
        }

        private void Sort(object parameter) => SortColumn = (SortColumn)(Enum.Parse(typeof(SortColumn), (string)parameter));

        private bool IsNotSynchronizing => _dataManager.IsSynchronizing == false;

        private bool CanImportCollection() => IsNotSynchronizing;

        private void ImportCollection()
        {
            SuspendEvents = true;

            _model.ImportCollection();

            ResumeEvents();
        }

        private void OnModelFilesChanged(object sender, EventArgs e)
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

        private void RaisePropertyChanged(string attribute) => _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));

        private void OnDataManagerIsSynchronizingChanged(object sender, EventArgs e)
        {
            RaisePropertyChanged(nameof(ImportCollectionCommand));
        }

        private void AddWatchedOn(object parameter)
        {
            var watchedOn = _windowFactory.OpenWatchedOnWindow();

            if (watchedOn.HasValue == false)
            {
                return;
            }

            SuspendEvents = true;

            GetEntries(parameter).ForEach(entry => _dataManager.AddWatched(entry, _userName, watchedOn.Value));

            _dataManager.SaveDataFile();

            ResumeEvents();
        }

        private void CheckForUpdate()
        {
            OnlineAccess.CheckForNewVersion("http://doena-soft.de/dvdprofiler/3.9.0/versions.xml", new WindowHandle(), "Watch History", GetType().Assembly);
        }

        private void ShowAbout()
        {
            using (var form = new AboutBox(GetType().Assembly))
            {
                form.ShowDialog();
            }
        }

        private void ShowHistory(object parameter)
        {
            var fileEntry = GetFileEntry(parameter);

            var watches = fileEntry.GetWatches(_userName).ToList();

            _windowFactory.OpenWatchesWindow(watches);
        }

        private void EditRunningTime(object parameter)
        {
            var fileEntry = GetFileEntry(parameter);

            var runningTime = _windowFactory.OpenRunningTimeWindow(fileEntry.VideoLength);

            if (runningTime.HasValue == false)
            {
                return;
            }

            fileEntry.VideoLength = runningTime.Value;

            _dataManager.SaveDataFile();
        }

        private bool CanAddYoutubeLink() => IsNotSynchronizing;

        private void AddYoutubeLink()
        {
            _windowFactory.OpenAddYoutubeLinkWindow(_userName);

            _dataManager.SaveDataFile();
        }

        private bool CanAddManualEntry() => IsNotSynchronizing;

        private void AddAddManualEntry()
        {
            _windowFactory.OpenAddManualEntryWindow(_userName);

            _dataManager.SaveDataFile();
        }

        private void EditTitle(object parameter)
        {
            var fileEntry = GetFileEntry(parameter);

            var title = _windowFactory.OpenEditTitleWindow(fileEntry.Title);

            if (title == null)
            {
                return;
            }

            fileEntry.Title = title;

            _dataManager.SaveDataFile();
        }

        private void ShowReport()
        {
            _windowFactory.OpenShowReportWindow(_userName);

            _dataManager.SaveDataFile();
        }
    }
}