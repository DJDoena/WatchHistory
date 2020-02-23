namespace DoenaSoft.WatchHistory.Settings.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Input;
    using AbstractionLayer.UIServices;
    using Data;
    using ToolBox.Commands;

    internal sealed class SettingsViewModel : ISettingsViewModel
    {
        private readonly IDataManager _dataManager;

        private readonly IUIServices _uiServices;

        private ISettingsListBoxItemViewModel _selectedUser;

        private string _selectedRootFolder;

        private ISettingsListBoxItemViewModel _selectedFileExtension;

        public SettingsViewModel(IDataManager dataManager, IUIServices uiServices)
        {
            _dataManager = dataManager;
            _uiServices = uiServices;

            Users = new ObservableCollection<ISettingsListBoxItemViewModel>(_dataManager.Users.Select(item => new SettingsListBoxItemViewModel(item)));

            RootFolders = new ObservableCollection<string>(_dataManager.RootFolders);

            FileExtensions = new ObservableCollection<ISettingsListBoxItemViewModel>(_dataManager.FileExtensions.Select(item => new SettingsListBoxItemViewModel(item)));

            AddUserCommand = new RelayCommand(AddUser);
            RemoveUserCommand = new RelayCommand(RemoveUser, CanRemoveUser);
            AddRootFolderCommand = new RelayCommand(AddRootFolder);
            RemoveRootFolderCommand = new RelayCommand(RemoveRootFolder, CanRemoveRootFolder);
            AddFileExtensionCommand = new RelayCommand(AddFileExtension);
            RemoveFileExtensionCommand = new RelayCommand(RemoveFileExtension, CanRemoveFileExtension);
            AcceptCommand = new RelayCommand(Accept);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region ISettingsViewModel

        public ObservableCollection<ISettingsListBoxItemViewModel> Users { get; private set; }

        public ISettingsListBoxItemViewModel SelectedUser
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

        public ICommand AddUserCommand { get; }

        public ICommand RemoveUserCommand { get; }

        public ObservableCollection<string> RootFolders { get; private set; }

        public string SelectedRootFolder
        {
            get => _selectedRootFolder;
            set
            {
                if (value != _selectedRootFolder)
                {
                    _selectedRootFolder = value;

                    RaisePropertyChanged(nameof(SelectedRootFolder));
                }
            }
        }

        public ICommand AddRootFolderCommand { get; }

        public ICommand RemoveRootFolderCommand { get; }

        public ObservableCollection<ISettingsListBoxItemViewModel> FileExtensions { get; private set; }

        public ISettingsListBoxItemViewModel SelectedFileExtension
        {
            get => _selectedFileExtension;
            set
            {
                if (value != _selectedFileExtension)
                {
                    _selectedFileExtension = value;

                    RaisePropertyChanged(nameof(SelectedFileExtension));
                }
            }
        }

        public ICommand AddFileExtensionCommand { get; }

        public ICommand RemoveFileExtensionCommand { get; }

        public ICommand AcceptCommand { get; }

        public event EventHandler Closing;

        #endregion

        private void AddUser() => Users.Add(new SettingsListBoxItemViewModel(string.Empty));

        private bool CanRemoveUser() => SelectedUser != null;

        private void RemoveUser() => Users.Remove(SelectedUser);

        private void AddRootFolder()
        {
            if (_uiServices.ShowFolderBrowserDialog(new FolderBrowserDialogOptions(), out string rootFolder))
            {
                RootFolders.Add(rootFolder);
            }
        }

        private bool CanRemoveRootFolder() => SelectedRootFolder != null;

        private void RemoveRootFolder() => RootFolders.Remove(SelectedRootFolder);

        private void AddFileExtension() => FileExtensions.Add(new SettingsListBoxItemViewModel(string.Empty));

        private bool CanRemoveFileExtension() => SelectedFileExtension != null;

        private void RemoveFileExtension() => FileExtensions.Remove(SelectedFileExtension);

        private void Accept()
        {
            var users = new HashSet<string>(Users.Select(item => item.Value));

            _dataManager.Users = users.Where(item => string.IsNullOrEmpty(item) == false);

            var rootFolders = new HashSet<string>(RootFolders);

            _dataManager.RootFolders = rootFolders.Where(item => string.IsNullOrEmpty(item) == false);

            var fileExtensions = new HashSet<string>(FileExtensions.Select(item => item.Value));

            _dataManager.FileExtensions = fileExtensions.Where(item => string.IsNullOrEmpty(item) == false);

            Closing?.Invoke(this, EventArgs.Empty);
        }

        private void RaisePropertyChanged(string attribute) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
    }
}