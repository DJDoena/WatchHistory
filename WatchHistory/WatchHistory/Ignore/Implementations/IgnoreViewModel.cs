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
        private readonly IIgnoreModel Model;

        private readonly IDataManager DataManager;

        private readonly IIOServices IOServices;

        private readonly IWindowFactory WindowFactory;

        private readonly String UserName;

        private event PropertyChangedEventHandler _PropertyChanged;

        public IgnoreViewModel(IIgnoreModel model
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
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (_PropertyChanged == null)
                {
                    Model.FilesChanged += OnModelFilesChanged;
                }

                _PropertyChanged += value;
            }
            remove
            {
                _PropertyChanged -= value;

                if (_PropertyChanged == null)
                {
                    Model.FilesChanged -= OnModelFilesChanged;
                }
            }
        }

        #endregion

        #region IIgnoreViewModel

        public String Title
            => $"Watch History (user: {UserName})";

        public String Filter
        {
            get => Model.Filter;
            set
            {
                if (value != Model.Filter)
                {
                    Model.Filter = value;

                    RaisePropertyChanged(nameof(Filter));
                }
            }
        }

        public ObservableCollection<IFileEntryViewModel> Entries
        {
            get
            {
                IEnumerable<FileEntry> modelEntries = Model.GetFiles();

                ObservableCollection<IFileEntryViewModel> viewModelEntries = ViewModelHelper.GetSortedEntries(modelEntries, UserName, DataManager, IOServices, SortColumn.File, true);

                return (viewModelEntries);
            }
        }

        public ICommand UndoIgnoreCommand
           => new ParameterizedRelayCommand(UndoIgnore);

        #endregion

        private void UndoIgnore(Object parameter)
        {
            IEnumerable<IFileEntryViewModel> entries = ((IList)parameter).Cast<IFileEntryViewModel>().ToList();

            entries.ForEach(entry => DataManager.UndoIgnore(entry.FileEntry, UserName));

            DataManager.SaveDataFile(WatchHistory.Environment.DataFile);
        }

        private void OnModelFilesChanged(Object sender
            , EventArgs e)
            => RaisePropertyChanged(nameof(Entries));

        private void RaisePropertyChanged(String attribute)
            => _PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
    }
}