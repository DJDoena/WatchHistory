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

        private readonly string _userName;

#pragma warning disable IDE1006 // Naming Styles
        private event PropertyChangedEventHandler _propertyChanged;
#pragma warning restore IDE1006 // Naming Styles

        public IgnoreViewModel(IIgnoreModel model
            , IDataManager dataManager
            , IIOServices ioServices
            , string userName)
        {
            _model = model;
            _dataManager = dataManager;
            _ioServices = ioServices;
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

        private void UndoIgnore(object parameter)
        {
            IEnumerable<IFileEntryViewModel> entries = ((IList)parameter).Cast<IFileEntryViewModel>().ToList();

            entries.ForEach(entry => _dataManager.UndoIgnore(entry.FileEntry, _userName));

            _dataManager.SaveDataFile();
        }

        private void OnModelFilesChanged(object sender
            , EventArgs e)
            => RaisePropertyChanged(nameof(Entries));

        private void RaisePropertyChanged(string attribute)
            => _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
    }
}