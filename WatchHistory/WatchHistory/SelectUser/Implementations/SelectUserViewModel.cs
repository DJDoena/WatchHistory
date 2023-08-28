using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using DoenaSoft.AbstractionLayer.Commands;
using DoenaSoft.WatchHistory.Data;

namespace DoenaSoft.WatchHistory.SelectUser.Implementations
{
    internal sealed class SelectUserViewModel : ISelectUserViewModel
    {
        private readonly IDataManager _dataManager;

        private readonly IWindowFactory _windowFactory;

        private string _selectedUser;

        public SelectUserViewModel(IDataManager dataManager, IWindowFactory windowFactory)
        {
            _dataManager = dataManager;
            _windowFactory = windowFactory;

            _selectedUser = _dataManager.Users.First();

            SelectCommand = new RelayCommand(Select);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region ISelectUserViewModel

        public IEnumerable<string> Users => _dataManager.Users;

        public string SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (value != _selectedUser)
                {
                    _selectedUser = value;

                    RaisePropertyChanged(nameof(SelectedUser));
                }
            }
        }

        public ICommand SelectCommand { get; }

        public event EventHandler Closing;

        #endregion

        private void Select()
        {
            _windowFactory.OpenMainWindow(SelectedUser);

            Closing?.Invoke(this, EventArgs.Empty);
        }

        private void RaisePropertyChanged(string attribute) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
    }
}