namespace DoenaSoft.WatchHistory.SelectUser.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Input;
    using Data;
    using ToolBox.Commands;

    internal sealed class SelectUserViewModel : ISelectUserViewModel
    {
        private readonly IDataManager DataManager;

        private readonly IWindowFactory WindowFactory;

        private String _SelectedUser;

        public SelectUserViewModel(IDataManager dataManager
            , IWindowFactory windowFactory)
        {
            DataManager = dataManager;
            WindowFactory = windowFactory;

            _SelectedUser = DataManager.Users.First();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region ISelectUserViewModel

        public IEnumerable<String> Users
            => (DataManager.Users);

        public String SelectedUser
        {
            get => _SelectedUser;
            set
            {
                if (value != _SelectedUser)
                {
                    _SelectedUser = value;

                    RaisePropertyChanged(nameof(SelectedUser));
                }
            }
        }

        public ICommand SelectCommand
            => new RelayCommand(Select);

        public event EventHandler Closing;

        #endregion

        private void Select()
        {
            WindowFactory.OpenMainWindow(SelectedUser);

            Closing?.Invoke(this, EventArgs.Empty);
        }

        private void RaisePropertyChanged(String attribute)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
    }
}