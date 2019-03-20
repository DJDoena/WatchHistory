namespace DoenaSoft.WatchHistory.Ignore.Implementations
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

    internal sealed class IgnoreViewModel : IIgnoreViewModel
    {
        private readonly IIgnoreModel _model;

        private readonly IDataManager _dataManager;

        private readonly IIOServices _ioServices;

        private readonly IWindowFactory _windowFactory;

        private readonly String _userName;

        private event PropertyChangedEventHandler _propertyChanged;

        public IgnoreViewModel(IIgnoreModel model
            , IDataManager dataManager
            , IIOServices ioServices
            , IWindowFactory windowFactory
            , String userName)
        {
            _model = model;
            _dataManager = dataManager;
            _ioServices = ioServices;
            _windowFactory = windowFactory;
            _userName = userName;

            UndoIgnoreCommand = new ParameterizedRelayCommand(UndoIgnore);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (_propertyChanged == null)
                {
                    _model.FilesChanged += OnModelFilesChanged;
                }

                _propertyChanged += value;
            }
            remove
            {
                _propertyChanged -= value;

                if (_propertyChanged == null)
                {
                    _model.FilesChanged -= OnModelFilesChanged;
                }
            }
        }

        #endregion

        #region IIgnoreViewModel

        public String Filter
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
                IEnumerable<FileEntry> modelEntries = _model.GetFiles();

                ObservableCollection<IFileEntryViewModel> viewModelEntries = ViewModelHelper.GetSortedEntries(modelEntries, _userName, _dataManager, _ioServices, SortColumn.File, true);

                return (viewModelEntries);
            }
        }

        public ICommand UndoIgnoreCommand { get; }

        #endregion

        private void UndoIgnore(Object parameter)
        {
            IEnumerable<IFileEntryViewModel> entries = ((IList)parameter).Cast<IFileEntryViewModel>().ToList();

            entries.ForEach(entry => _dataManager.UndoIgnore(entry.FileEntry, _userName));

            _dataManager.SaveDataFile();
        }

        private void OnModelFilesChanged(Object sender
            , EventArgs e)
            => RaisePropertyChanged(nameof(Entries));

        private void RaisePropertyChanged(String attribute)
            => _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
    }
}