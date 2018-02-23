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
        private readonly IMainModel _Model;

        private readonly IDataManager _DataManager;

        private readonly IIOServices _IOServices;

        private readonly IWindowFactory _WindowFactory;

        private readonly String _UserName;

        private readonly ICommand _AddWatchedCommand;

        private readonly ICommand _PlayFileAndAddWatchedCommand;

        private readonly ICommand _IgnoreCommand;

        private readonly ICommand _OpenSettingsCommand;

        private readonly ICommand _ImportCollectionCommand;

        private readonly ICommand _UndoIgnoreCommand;

        private readonly ICommand _PlayFileCommand;

        private readonly ICommand _SortCommand;

        private readonly ICommand _OpenFileLocationCommand;

        private readonly ICommand _AddWatchedOnCommand;

        private event PropertyChangedEventHandler _PropertyChanged;
        
        private SortColumn _SortColumn;

        private Boolean SortAscending { get; set; }

        private Boolean SuspendEvents { get; set; }

        private Boolean EventRaisedWhileSuspended { get; set; }

        private SortColumn SortColumn
        {
            get => _SortColumn;
            set
            {
                SortAscending = (value != _SortColumn) ? true : (SortAscending == false);

                _SortColumn = value;

                RaisePropertyChanged(nameof(Entries));
            }
        }

        public MainViewModel(IMainModel model
            , IDataManager dataManager
            , IIOServices ioServices
            , IWindowFactory windowFactory
            , String userName)
        {
            _Model = model;
            _DataManager = dataManager;
            _IOServices = ioServices;
            _WindowFactory = windowFactory;
            _UserName = userName;

            _SortColumn = SortColumn.CreationTime;
            SortAscending = false;
            SuspendEvents = false;
            EventRaisedWhileSuspended = false;

            _AddWatchedCommand = new ParameterizedRelayCommand(AddWatched);
            _PlayFileAndAddWatchedCommand = new ParameterizedRelayCommand(PlayFileAndAddWatched, CanPlayFile);
            _OpenSettingsCommand = new RelayCommand(OpenSettings);
            _ImportCollectionCommand = new RelayCommand(ImportCollection, CanImportCollection);
            _IgnoreCommand = new ParameterizedRelayCommand(Ignore);
            _UndoIgnoreCommand = new RelayCommand(UndoIgnore);
            _PlayFileCommand = new ParameterizedRelayCommand(PlayFile, CanPlayFile);
            _SortCommand = new ParameterizedRelayCommand(Sort);
            _OpenFileLocationCommand = new ParameterizedRelayCommand(OpenFileLocation);
            _AddWatchedOnCommand = new ParameterizedRelayCommand(AddWatchedOn);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (_PropertyChanged == null)
                {
                    _Model.FilesChanged += OnModelFilesChanged;

                    _DataManager.IsSynchronizingChanged += OnDataManagerIsSynchronizingChanged;
                }

                _PropertyChanged += value;
            }
            remove
            {
                _PropertyChanged -= value;

                if (_PropertyChanged == null)
                {
                    _DataManager.IsSynchronizingChanged -= OnDataManagerIsSynchronizingChanged;

                    _Model.FilesChanged -= OnModelFilesChanged;
                }
            }
        }

        #endregion

        #region IMainViewModel

        public String Title
            => $"Watch History (user: {_UserName})";

        public String Filter
        {
            get => _Model.Filter;
            set
            {
                if (value != _Model.Filter)
                {
                    _Model.Filter = value;

                    RaisePropertyChanged(nameof(Filter));
                }
            }
        }

        public Boolean IgnoreWatched
        {
            get => _Model.IgnoreWatched;
            set
            {
                if (value != _Model.IgnoreWatched)
                {
                    _Model.IgnoreWatched = value;

                    RaisePropertyChanged(nameof(IgnoreWatched));
                }
            }
        }

        public ObservableCollection<IFileEntryViewModel> Entries
        {
            get
            {
                IEnumerable<FileEntry> modelEntries = _Model.GetFiles();

                ObservableCollection<IFileEntryViewModel> viewModelEntries = ViewModelHelper.GetSortedEntries(modelEntries, _UserName, _DataManager, _IOServices, SortColumn, SortAscending);

                return (viewModelEntries);
            }
        }

        public ICommand AddWatchedCommand
            => _AddWatchedCommand;

        public ICommand PlayFileAndAddWatchedCommand
            => _PlayFileAndAddWatchedCommand;

        public ICommand OpenSettingsCommand
            => _OpenSettingsCommand;

        public ICommand ImportCollectionCommand
            => _ImportCollectionCommand;

        public ICommand IgnoreCommand
           => _IgnoreCommand;

        public ICommand UndoIgnoreCommand
            => _UndoIgnoreCommand;

        public ICommand PlayFileCommand
            => _PlayFileCommand;

        public ICommand SortCommand
            => _SortCommand;

        public ICommand OpenFileLocationCommand
            => _OpenFileLocationCommand;

        public ICommand AddWatchedOnCommand
            => _AddWatchedOnCommand;

        #endregion

        private void AddWatched(Object parameter)
        {
            SuspendEvents = true;

            GetEntries(parameter).ForEach(entry => _DataManager.AddWatched(entry, _UserName));

            _DataManager.SaveDataFile(WatchHistory.Environment.DataFile);

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
            => ((IList)parameter).Cast<IFileEntryViewModel>().Select(entry => entry.FileEntry).ToList();

        private void Ignore(Object parameter)
        {
            SuspendEvents = true;

            GetEntries(parameter).ForEach(entry => _DataManager.AddIgnore(entry, _UserName));

            _DataManager.SaveDataFile(WatchHistory.Environment.DataFile);

            ResumeEvents();
        }

        private void OpenSettings()
            => _WindowFactory.OpenSettingsWindow();

        private void UndoIgnore()
            => _WindowFactory.OpenIgnoreWindow(_UserName);

        private void PlayFile(Object parameter)
        {
            FileEntry fileEntry = GetFileEntry(parameter);

            _Model.PlayFile(fileEntry);
        }

        private Boolean CanPlayFile(Object parameter)
        {
            FileEntry fileEntry = GetFileEntry(parameter);

            Boolean canPlay = (fileEntry != null) && (_Model.CanPlayFile(fileEntry));

            return (canPlay);
        }

        private void PlayFileAndAddWatched(Object parameter)
        {
            FileEntry fileEntry = GetFileEntry(parameter);

            _Model.PlayFile(fileEntry);

            _DataManager.AddWatched(fileEntry, _UserName);

            _DataManager.SaveDataFile(WatchHistory.Environment.DataFile);
        }

        private static FileEntry GetFileEntry(Object parameter)
            => ((IFileEntryViewModel)parameter)?.FileEntry;

        private void OpenFileLocation(Object parameter)
        {
            FileEntry fileEntry = GetFileEntry(parameter);

            _Model.OpenFileLocation(fileEntry);
        }

        private void Sort(Object parameter)
            => SortColumn = (SortColumn)(Enum.Parse(typeof(SortColumn), (String)parameter));

        private Boolean CanImportCollection()
            => _DataManager.IsSynchronizing == false;

        private void ImportCollection()
        {
            SuspendEvents = true;

            _Model.ImportCollection();

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
            => _PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));

        private void OnDataManagerIsSynchronizingChanged(Object sender
            , EventArgs e)
        {
            RaisePropertyChanged(nameof(ImportCollectionCommand));
        }

        private void AddWatchedOn(Object parameter)
        {
            Nullable<DateTime> watchedOn = _WindowFactory.OpenWatchedOnWindow();

            if (watchedOn.HasValue == false)
            {
                return;
            }
            
            SuspendEvents = true;

            GetEntries(parameter).ForEach(entry => _DataManager.AddWatched(entry, _UserName, watchedOn.Value));

            _DataManager.SaveDataFile(WatchHistory.Environment.DataFile);

            ResumeEvents();
        }
    }
}