using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using DoenaSoft.AbstractionLayer.Commands;
using DoenaSoft.AbstractionLayer.UIServices;
using DoenaSoft.WatchHistory.Data;

namespace DoenaSoft.WatchHistory.Settings.Implementations
{
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

            this.Users = new ObservableCollection<ISettingsListBoxItemViewModel>(_dataManager.Users.Select(item => new SettingsListBoxItemViewModel(item)));

            this.RootFolders = new ObservableCollection<string>(_dataManager.RootFolders);

            this.FileExtensions = new ObservableCollection<ISettingsListBoxItemViewModel>(_dataManager.FileExtensions.Select(item => new SettingsListBoxItemViewModel(item)));

            this.AddUserCommand = new RelayCommand(this.AddUser);
            this.RemoveUserCommand = new RelayCommand(this.RemoveUser, this.CanRemoveUser);
            this.AddRootFolderCommand = new RelayCommand(this.AddRootFolder);
            this.RemoveRootFolderCommand = new RelayCommand(this.RemoveRootFolder, this.CanRemoveRootFolder);
            this.AddFileExtensionCommand = new RelayCommand(this.AddFileExtension);
            this.RemoveFileExtensionCommand = new RelayCommand(this.RemoveFileExtension, this.CanRemoveFileExtension);
            this.AcceptCommand = new RelayCommand(this.Accept);
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

                    this.RaisePropertyChanged(nameof(this.SelectedUser));
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

                    this.RaisePropertyChanged(nameof(this.SelectedRootFolder));
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

                    this.RaisePropertyChanged(nameof(this.SelectedFileExtension));
                }
            }
        }

        public ICommand AddFileExtensionCommand { get; }

        public ICommand RemoveFileExtensionCommand { get; }

        public ICommand AcceptCommand { get; }

        public event EventHandler Closing;

        #endregion

        private void AddUser() => this.Users.Add(new SettingsListBoxItemViewModel(string.Empty));

        private bool CanRemoveUser() => this.SelectedUser != null;

        private void RemoveUser() => this.Users.Remove(this.SelectedUser);

        private void AddRootFolder()
        {
            if (_uiServices.ShowFolderBrowserDialog(new FolderBrowserDialogOptions(), out var rootFolder))
            {
                this.RootFolders.Add(rootFolder);
            }
        }

        private bool CanRemoveRootFolder() => this.SelectedRootFolder != null;

        private void RemoveRootFolder() => this.RootFolders.Remove(this.SelectedRootFolder);

        private void AddFileExtension() => this.FileExtensions.Add(new SettingsListBoxItemViewModel(string.Empty));

        private bool CanRemoveFileExtension() => this.SelectedFileExtension != null;

        private void RemoveFileExtension() => this.FileExtensions.Remove(this.SelectedFileExtension);

        private void Accept()
        {
            var users = new HashSet<string>(this.Users.Select(item => item.Value));

            _dataManager.Users = users.Where(item => string.IsNullOrEmpty(item) == false);

            var rootFolders = new HashSet<string>(this.RootFolders);

            _dataManager.RootFolders = rootFolders.Where(item => string.IsNullOrEmpty(item) == false);

            var fileExtensions = new HashSet<string>(this.FileExtensions.Select(item => item.Value));

            _dataManager.FileExtensions = fileExtensions.Where(item => string.IsNullOrEmpty(item) == false);

            Closing?.Invoke(this, EventArgs.Empty);
        }

        private void RaisePropertyChanged(string attribute) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
    }
}