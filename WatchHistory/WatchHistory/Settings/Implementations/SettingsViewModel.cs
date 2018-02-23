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
        private readonly IDataManager _DataManager;

        private readonly IUIServices _UIServices;

        private readonly ICommand _AddUserCommand;

        private readonly ICommand _RemoveUserCommand;

        private readonly ICommand _AddRootFolderCommand;

        private readonly ICommand _RemoveRootFolderCommand;

        private readonly ICommand _AddFileExtensionCommand;

        private readonly ICommand _RemoveFileExtensionCommand;

        private readonly ICommand _AcceptCommand;

        private ISettingsListBoxItemViewModel _SelectedUser;

        private String _SelectedRootFolder;

        private ISettingsListBoxItemViewModel _SelectedFileExtension;

        public SettingsViewModel(IDataManager dataManager
            , IUIServices uiServices)
        {
            _DataManager = dataManager;
            _UIServices = uiServices;

            Users = new ObservableCollection<ISettingsListBoxItemViewModel>(_DataManager.Users.Select(item => new SettingsListBoxItemViewModel(item)));

            RootFolders = new ObservableCollection<String>(_DataManager.RootFolders);

            FileExtensions = new ObservableCollection<ISettingsListBoxItemViewModel>(_DataManager.FileExtensions.Select(item => new SettingsListBoxItemViewModel(item)));

            _AddUserCommand = new RelayCommand(AddUser);
            _RemoveUserCommand = new RelayCommand(RemoveUser, CanRemoveUser);
            _AddRootFolderCommand = new RelayCommand(AddRootFolder);
            _RemoveRootFolderCommand = new RelayCommand(RemoveRootFolder, CanRemoveRootFolder);
            _AddFileExtensionCommand = new RelayCommand(AddFileExtension);
            _RemoveFileExtensionCommand = new RelayCommand(RemoveFileExtension, CanRemoveFileExtension);
            _AcceptCommand = new RelayCommand(Accept);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region ISettingsViewModel

        public ObservableCollection<ISettingsListBoxItemViewModel> Users { get; private set; }

        public ISettingsListBoxItemViewModel SelectedUser
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

        public ICommand AddUserCommand
            => _AddUserCommand;

        public ICommand RemoveUserCommand
            => _RemoveUserCommand;

        public ObservableCollection<String> RootFolders { get; private set; }

        public String SelectedRootFolder
        {
            get => _SelectedRootFolder;
            set
            {
                if (value != _SelectedRootFolder)
                {
                    _SelectedRootFolder = value;

                    RaisePropertyChanged(nameof(SelectedRootFolder));
                }
            }
        }

        public ICommand AddRootFolderCommand
            => _AddRootFolderCommand;

        public ICommand RemoveRootFolderCommand
            => _RemoveRootFolderCommand;

        public ObservableCollection<ISettingsListBoxItemViewModel> FileExtensions { get; private set; }

        public ISettingsListBoxItemViewModel SelectedFileExtension
        {
            get => _SelectedFileExtension;
            set
            {
                if (value != _SelectedFileExtension)
                {
                    _SelectedFileExtension = value;

                    RaisePropertyChanged(nameof(SelectedFileExtension));
                }
            }
        }

        public ICommand AddFileExtensionCommand
            => _AddFileExtensionCommand;

        public ICommand RemoveFileExtensionCommand
            => _RemoveFileExtensionCommand;

        public ICommand AcceptCommand
            => _AcceptCommand;

        public event EventHandler Closing;

        #endregion

        private void AddUser()
            => Users.Add(new SettingsListBoxItemViewModel(String.Empty));

        private Boolean CanRemoveUser()
            => SelectedUser != null;

        private void RemoveUser()
            => Users.Remove(SelectedUser);

        private void AddRootFolder()
        {
            if (_UIServices.ShowFolderBrowserDialog(new FolderBrowserDialogOptions(), out string rootFolder))
            {
                RootFolders.Add(rootFolder);
            }
        }

        private Boolean CanRemoveRootFolder()
            => SelectedRootFolder != null;

        private void RemoveRootFolder()
            => RootFolders.Remove(SelectedRootFolder);

        private void AddFileExtension()
            => FileExtensions.Add(new SettingsListBoxItemViewModel(String.Empty));

        private Boolean CanRemoveFileExtension()
            => SelectedFileExtension != null;

        private void RemoveFileExtension()
            => FileExtensions.Remove(SelectedFileExtension);

        private void Accept()
        {
            HashSet<String> users = new HashSet<String>(Users.Select(item => item.Value));

            _DataManager.Users = users.Where(item => String.IsNullOrEmpty(item) == false);

            HashSet<String> rootFolders = new HashSet<String>(RootFolders);

            _DataManager.RootFolders = rootFolders.Where(item => String.IsNullOrEmpty(item) == false);

            HashSet<String> fileExtensions = new HashSet<String>(FileExtensions.Select(item => item.Value));

            _DataManager.FileExtensions = fileExtensions.Where(item => String.IsNullOrEmpty(item) == false);

            Closing?.Invoke(this, EventArgs.Empty);
        }

        private void RaisePropertyChanged(String attribute)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
    }
}