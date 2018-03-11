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
        private readonly IIgnoreModel _Model;

        private readonly IDataManager _DataManager;

        private readonly IIOServices _IOServices;

        private readonly IWindowFactory _WindowFactory;

        private readonly String _UserName;

        private readonly ICommand _UndoIgnoreCommand;

        private event PropertyChangedEventHandler _PropertyChanged;

        public IgnoreViewModel(IIgnoreModel model
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

            _UndoIgnoreCommand = new ParameterizedRelayCommand(UndoIgnore);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (_PropertyChanged == null)
                {
                    _Model.FilesChanged += OnModelFilesChanged;
                }

                _PropertyChanged += value;
            }
            remove
            {
                _PropertyChanged -= value;

                if (_PropertyChanged == null)
                {
                    _Model.FilesChanged -= OnModelFilesChanged;
                }
            }
        }

        #endregion

        #region IIgnoreViewModel

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

        public ObservableCollection<IFileEntryViewModel> Entries
        {
            get
            {
                IEnumerable<FileEntry> modelEntries = _Model.GetFiles();

                ObservableCollection<IFileEntryViewModel> viewModelEntries = ViewModelHelper.GetSortedEntries(modelEntries, _UserName, _DataManager, _IOServices, SortColumn.File, true);

                return (viewModelEntries);
            }
        }

        public ICommand UndoIgnoreCommand
           => _UndoIgnoreCommand;

        #endregion

        private void UndoIgnore(Object parameter)
        {
            IEnumerable<IFileEntryViewModel> entries = ((IList)parameter).Cast<IFileEntryViewModel>().ToList();

            entries.ForEach(entry => _DataManager.UndoIgnore(entry.FileEntry, _UserName));

            _DataManager.SaveDataFile();
        }

        private void OnModelFilesChanged(Object sender
            , EventArgs e)
            => RaisePropertyChanged(nameof(Entries));

        private void RaisePropertyChanged(String attribute)
            => _PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
    }
}