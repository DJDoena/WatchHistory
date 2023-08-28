using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using DoenaSoft.AbstractionLayer.Commands;
using DoenaSoft.AbstractionLayer.IOServices;
using DoenaSoft.DVDProfiler.DVDProfilerHelper;
using DoenaSoft.ToolBox.Extensions;
using DoenaSoft.WatchHistory.Data;
using DoenaSoft.WatchHistory.Data.Implementations;
using DoenaSoft.WatchHistory.Implementations;
using MIHC = DoenaSoft.MediaInfoHelper.Helpers.Constants;

namespace DoenaSoft.WatchHistory.Main.Implementations
{
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
                this.SortAscending = (value != _sortColumn) ? true : (this.SortAscending == false);

                _sortColumn = value;

                this.RaisePropertyChanged(nameof(this.Entries));
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
            this.SortAscending = false;
            this.SuspendEvents = false;
            this.EventRaisedWhileSuspended = false;

            this.AddWatchedCommand = new ParameterizedRelayCommand(this.AddWatched, this.CanAddWatched);
            this.PlayFileAndAddWatchedCommand = new ParameterizedRelayCommand(this.PlayFileAndAddWatched, this.CanPlayFile);
            this.OpenSettingsCommand = new RelayCommand(this.OpenSettings);
            this.ImportCollectionCommand = new RelayCommand(this.ImportCollection, this.CanImportCollection);
            this.IgnoreCommand = new ParameterizedRelayCommand(this.Ignore, this.CanIgnore);
            this.UndoIgnoreCommand = new RelayCommand(this.UndoIgnore);
            this.PlayFileCommand = new ParameterizedRelayCommand(this.PlayFile, this.CanPlayFile);
            this.SortCommand = new ParameterizedRelayCommand(this.Sort);
            this.OpenFileLocationCommand = new ParameterizedRelayCommand(this.OpenFileLocation, this.CanOpenFileLocation);
            this.AddWatchedOnCommand = new ParameterizedRelayCommand(this.AddWatchedOn, this.CanAddWatchedOn);
            this.CheckForUpdateCommand = new RelayCommand(this.CheckForUpdate);
            this.AboutCommand = new RelayCommand(this.ShowAbout);
            this.ShowHistoryCommand = new ParameterizedRelayCommand(this.ShowHistory, this.CanShowHistory);
            this.EditRunningTimeCommand = new ParameterizedRelayCommand(this.EditRunningTime, this.CanEditRunningTime);
            this.AddYoutubeLinkCommand = new RelayCommand(this.AddYoutubeLink, this.CanAddYoutubeLink);
            this.AddManualEntryCommand = new RelayCommand(this.AddAddManualEntry, this.CanAddManualEntry);
            this.EditTitleCommand = new ParameterizedRelayCommand(this.EditTitle, this.CanEditTitle);
            this.ShowReportCommand = new RelayCommand(this.ShowReport);
            this.EditNoteCommand = new ParameterizedRelayCommand(this.EditNote, this.CanEditNote);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (_propertyChanged == null)
                {
                    _model.FilesChanged += this.OnModelFilesChanged;

                    _dataManager.IsSynchronizingChanged += this.OnDataManagerIsSynchronizingChanged;
                }

                _propertyChanged += value;
            }
            remove
            {
                _propertyChanged -= value;

                if (_propertyChanged == null)
                {
                    _dataManager.IsSynchronizingChanged -= this.OnDataManagerIsSynchronizingChanged;

                    _model.FilesChanged -= this.OnModelFilesChanged;
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

                    this.RaisePropertyChanged(nameof(this.Filter));
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

                    this.RaisePropertyChanged(nameof(this.IgnoreWatched));
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

                    this.RaisePropertyChanged(nameof(this.SearchInPath));
                }
            }
        }

        public ObservableCollection<IFileEntryViewModel> Entries
        {
            get
            {
                var modelEntries = _model.GetFiles();

                var viewModelEntries = ViewModelHelper.GetSortedEntries(modelEntries, _userName, _dataManager, _ioServices, this.SortColumn, this.SortAscending);

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

        public ICommand EditNoteCommand { get; }

        #endregion

        private bool CanAddWatched(object parameter) => GetFileEntries(parameter).Any();

        private void AddWatched(object parameter)
        {
            if (!this.CanAddWatched(parameter))
            {
                return;
            }

            this.SuspendEvents = true;

            GetFileEntries(parameter).ForEach(entry => _dataManager.AddWatched(entry, _userName));

            _dataManager.SaveDataFile();

            this.ResumeEvents();
        }

        private void ResumeEvents()
        {
            this.SuspendEvents = false;

            if (this.EventRaisedWhileSuspended)
            {
                this.OnModelFilesChanged(this, EventArgs.Empty);

                this.EventRaisedWhileSuspended = false;
            }
        }

        private static IEnumerable<FileEntry> GetFileEntries(object parameter) => ((parameter as IEnumerable) ?? Enumerable.Empty<IFileEntryViewModel>()).OfType<IFileEntryViewModel>().Select(entry => entry.Entry).ToList();

        private bool CanIgnore(object parameter) => GetFileEntries(parameter).Any();

        private void Ignore(object parameter)
        {
            if (!this.CanIgnore(parameter))
            {
                return;
            }

            this.SuspendEvents = true;

            GetFileEntries(parameter).ForEach(entry => _dataManager.AddIgnore(entry, _userName));

            _dataManager.SaveDataFile();

            this.ResumeEvents();
        }

        private void OpenSettings() => _windowFactory.OpenSettingsWindow();

        private void UndoIgnore() => _windowFactory.OpenIgnoreWindow(_userName, this.Filter);

        private bool CanPlayFile(object parameter)
        {
            var fileEntry = GetFileEntry(parameter);

            var canPlay = (fileEntry != null) && (_model.CanPlayFile(fileEntry));

            return canPlay;
        }

        private void PlayFile(object parameter)
        {
            if (!this.CanPlayFile(parameter))
            {
                return;
            }

            var fileEntry = GetFileEntry(parameter);

            _model.PlayFile(fileEntry);
        }

        private void PlayFileAndAddWatched(object parameter)
        {
            if (!this.CanPlayFile(parameter))
            {
                return;
            }

            var fileEntry = GetFileEntry(parameter);

            _model.PlayFile(fileEntry);

            _dataManager.AddWatched(fileEntry, _userName);

            _dataManager.SaveDataFile();
        }

        private static FileEntry GetFileEntry(object parameter) => (parameter as IFileEntryViewModel)?.Entry;

        private bool CanOpenFileLocation(object parameter)
        {
            var fileEntry = GetFileEntry(parameter);

            var canOpen = (fileEntry != null) && (_model.CanOpenFileLocation(fileEntry));

            return canOpen;
        }

        private void OpenFileLocation(object parameter)
        {
            if (!this.CanOpenFileLocation(parameter))
            {
                return;
            }

            var fileEntry = GetFileEntry(parameter);

            _model.OpenFileLocation(fileEntry);
        }

        private void Sort(object parameter) => this.SortColumn = (SortColumn)(Enum.Parse(typeof(SortColumn), (string)parameter));

        private bool IsNotSynchronizing => _dataManager.IsSynchronizing == false;

        private bool CanImportCollection() => this.IsNotSynchronizing;

        private void ImportCollection()
        {
            if (!this.CanImportCollection())
            {
                return;
            }

            this.SuspendEvents = true;

            _model.ImportCollection();

            this.ResumeEvents();
        }

        private void OnModelFilesChanged(object sender, EventArgs e)
        {
            if (this.SuspendEvents == false)
            {
                this.RaisePropertyChanged(nameof(this.Entries));
            }
            else
            {
                this.EventRaisedWhileSuspended = true;
            }
        }

        private void RaisePropertyChanged(string attribute) => _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));

        private void OnDataManagerIsSynchronizingChanged(object sender, EventArgs e)
        {
            this.RaisePropertyChanged(nameof(this.ImportCollectionCommand));
        }

        private bool CanAddWatchedOn(object parameter) => GetFileEntries(parameter).Any();

        private void AddWatchedOn(object parameter)
        {
            if (!this.CanAddWatchedOn(parameter))
            {
                return;
            }

            var watchedOn = _windowFactory.OpenWatchedOnWindow();

            if (watchedOn.HasValue == false)
            {
                return;
            }

            this.SuspendEvents = true;

            GetFileEntries(parameter).ForEach(entry => _dataManager.AddWatched(entry, _userName, watchedOn.Value));

            _dataManager.SaveDataFile();

            this.ResumeEvents();
        }

        private void CheckForUpdate()
        {
            OnlineAccess.CheckForNewVersion("http://doena-soft.de/dvdprofiler/3.9.0/versions.xml", new WindowHandle(), "Watch History", this.GetType().Assembly);
        }

        private void ShowAbout()
        {
            using (var form = new AboutBox(this.GetType().Assembly))
            {
                form.ShowDialog();
            }
        }

        private bool CanShowHistory(object parameter) => GetFileEntry(parameter) != null;

        private void ShowHistory(object parameter)
        {
            if (!this.CanShowHistory(parameter))
            {
                return;
            }

            var fileEntry = GetFileEntry(parameter);

            var watches = fileEntry.GetWatches(_userName).ToList();

            _windowFactory.OpenWatchesWindow(watches);
        }

        private bool CanEditRunningTime(object parameter) => GetFileEntry(parameter) != null;

        private void EditRunningTime(object parameter)
        {
            if (!this.CanEditRunningTime(parameter))
            {
                return;
            }

            var fileEntry = GetFileEntry(parameter);

            var runningTime = _windowFactory.OpenRunningTimeWindow(fileEntry.VideoLength);

            if (runningTime.HasValue == false)
            {
                return;
            }

            fileEntry.VideoLength = runningTime.Value;

            _dataManager.SaveDataFile();

            this.OnModelFilesChanged(this, EventArgs.Empty);
        }

        private bool CanAddYoutubeLink() => false; //IsNotSynchronizing;

        private void AddYoutubeLink()
        {
            if (!this.CanAddYoutubeLink())
            {
                return;
            }

            _windowFactory.OpenAddYoutubeLinkWindow(_userName);

            _dataManager.SaveDataFile();
        }

        private bool CanAddManualEntry() => this.IsNotSynchronizing;

        private void AddAddManualEntry()
        {
            if (!this.CanAddManualEntry())
            {
                return;
            }

            _windowFactory.OpenAddManualEntryWindow(_userName);

            _dataManager.SaveDataFile();
        }

        private bool CanEditTitle(object parameter) => GetFileEntry(parameter) != null;

        private void EditTitle(object parameter)
        {
            if (!this.CanEditTitle(parameter))
            {
                return;
            }

            var fileEntry = GetFileEntry(parameter);

            var title = _windowFactory.OpenEditTitleWindow(fileEntry);

            if (title == null)
            {
                return;
            }

            fileEntry.Title = title;

            _dataManager.SaveDataFile();

            this.OnModelFilesChanged(this, EventArgs.Empty);
        }

        private void ShowReport()
        {
            _windowFactory.OpenShowReportWindow(_userName);

            _dataManager.SaveDataFile();
        }

        private bool CanEditNote(object parameter) => GetFileEntry(parameter) != null;

        private void EditNote(object parameter)
        {
            if (!this.CanEditTitle(parameter))
            {
                return;
            }

            var fileEntry = GetFileEntry(parameter);

            var note = _windowFactory.OpenEditNoteWindow(fileEntry.Note);

            if (note == null)
            {
                return;
            }

            fileEntry.Note = note;

            var manualFolder = _ioServices.Path.Combine(WatchHistory.Environment.MyDocumentsFolder, "Manual");

            if (fileEntry.FullName.StartsWith(manualFolder) && fileEntry.FullName.EndsWith(MIHC.ManualFileExtension))
            {
                var info = (new ManualWatchesProcessor(_ioServices)).TryGetInfo(fileEntry);

                if (info == null)
                {
                    info = ManualWatchesProcessor.CreateInfo(fileEntry.Title, fileEntry.VideoLength, note);
                }
                else
                {
                    info.Note = note;
                }

                SerializerHelper.Serialize(_ioServices, fileEntry.FullName, info);
            }

            _dataManager.SaveDataFile();
        }
    }
}