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
        private readonly IDataManager _DataManager;

        private readonly IWindowFactory _WindowFactory;

        private String _SelectedUser;

        public SelectUserViewModel(IDataManager dataManager
            , IWindowFactory windowFactory)
        {
            _DataManager = dataManager;
            _WindowFactory = windowFactory;

            _SelectedUser = _DataManager.Users.First();

            SelectCommand = new RelayCommand(Select);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region ISelectUserViewModel

        public IEnumerable<String> Users
            => (_DataManager.Users);

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

        public ICommand SelectCommand { get; }

        public event EventHandler Closing;

        #endregion

        private void Select()
        {
            _WindowFactory.OpenMainWindow(SelectedUser);

            Closing?.Invoke(this, EventArgs.Empty);
        }

        private void RaisePropertyChanged(String attribute)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
    }
}