using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using DoenaSoft.AbstractionLayer.Commands;
using DoenaSoft.AbstractionLayer.IOServices;
using DoenaSoft.WatchHistory.Data;
using DoenaSoft.WatchHistory.Implementations;

namespace DoenaSoft.WatchHistory.IgnoreEntry.Implementations
{
    internal sealed class IgnoreEntryViewModel : IIgnoreEntryViewModel
    {
        private readonly IIgnoreEntryModel _model;

        private readonly IDataManager _dataManager;

        private readonly IIOServices _ioServices;

        private readonly string _userName;

#pragma warning disable IDE1006 // Naming Styles
        private event PropertyChangedEventHandler _propertyChanged;
#pragma warning restore IDE1006 // Naming Styles

        public IgnoreEntryViewModel(IIgnoreEntryModel model, IDataManager dataManager, IIOServices ioServices, string userName)
        {
            _model = model;
            _dataManager = dataManager;
            _ioServices = ioServices;
            _userName = userName;

            this.UndoIgnoreCommand = new ParameterizedRelayCommand(this.UndoIgnore);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (_propertyChanged == null)
                {
                    _model.FilesChanged += this.OnModelFilesChanged;
                }

                _propertyChanged += value;
            }
            remove
            {
                _propertyChanged -= value;

                if (_propertyChanged == null)
                {
                    _model.FilesChanged -= this.OnModelFilesChanged;
                }
            }
        }

        #endregion

        #region IIgnoreEntryViewModel

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

                var viewModelEntries = ViewModelHelper.GetSortedEntries(modelEntries, _userName, _dataManager, _ioServices, SortColumn.File, true);

                return viewModelEntries;
            }
        }

        public ICommand UndoIgnoreCommand { get; }

        #endregion

        private void UndoIgnore(object parameter)
        {
            var entries = ((IList)parameter).Cast<IFileEntryViewModel>().ToList();

            entries.ForEach(entry => _dataManager.UndoIgnore(entry.Entry, _userName));

            _dataManager.SaveDataFile();
        }

        private void OnModelFilesChanged(object sender, EventArgs e) => this.RaisePropertyChanged(nameof(this.Entries));

        private void RaisePropertyChanged(string attribute) => _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
    }
}