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
    using ToolBox.Extensions;

    internal sealed class SettingsViewModel : ISettingsViewModel
    {
        private readonly IDataManager DataManager;

        private readonly IUIServices UIServices;

        private ISettingsListBoxItemViewModel m_SelectedUser;

        private String m_SelectedRootFolder;

        private ISettingsListBoxItemViewModel m_SelectedFileExtension;

        public SettingsViewModel(IDataManager dataManager
            , IUIServices uiServices)
        {
            DataManager = dataManager;
            UIServices = uiServices;

            Users = new ObservableCollection<ISettingsListBoxItemViewModel>(DataManager.Users.ForEach(item => new SettingsListBoxItemViewModel(item)));

            RootFolders = new ObservableCollection<String>(DataManager.RootFolders);

            FileExtensions = new ObservableCollection<ISettingsListBoxItemViewModel>(DataManager.FileExtensions.ForEach(item => new SettingsListBoxItemViewModel(item)));
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region ISettingsViewModel

        public ObservableCollection<ISettingsListBoxItemViewModel> Users { get; private set; }

        public ISettingsListBoxItemViewModel SelectedUser
        {
            get
            {
                return (m_SelectedUser);
            }
            set
            {
                if (value != m_SelectedUser)
                {
                    m_SelectedUser = value;

                    RaisePropertyChanged(nameof(SelectedUser));
                }
            }
        }

        public ICommand AddUserCommand
            => (new RelayCommand(AddUser));

        public ICommand RemoveUserCommand
            => (new RelayCommand(RemoveUser, CanRemoveUser));

        public ObservableCollection<String> RootFolders { get; private set; }

        public String SelectedRootFolder
        {
            get
            {
                return (m_SelectedRootFolder);
            }
            set
            {
                if (value != m_SelectedRootFolder)
                {
                    m_SelectedRootFolder = value;

                    RaisePropertyChanged(nameof(SelectedRootFolder));
                }
            }
        }

        public ICommand AddRootFolderCommand
            => (new RelayCommand(AddRootFolder));

        public ICommand RemoveRootFolderCommand
            => (new RelayCommand(RemoveRootFolder, CanRemoveRootFolder));

        public ObservableCollection<ISettingsListBoxItemViewModel> FileExtensions { get; private set; }

        public ISettingsListBoxItemViewModel SelectedFileExtension
        {
            get
            {
                return (m_SelectedFileExtension);
            }
            set
            {
                if (value != m_SelectedFileExtension)
                {
                    m_SelectedFileExtension = value;

                    RaisePropertyChanged(nameof(SelectedFileExtension));
                }
            }
        }

        public ICommand AddFileExtensionCommand
            => (new RelayCommand(AddFileExtension));

        public ICommand RemoveFileExtensionCommand
            => (new RelayCommand(RemoveFileExtension, CanRemoveFileExtension));

        public ICommand AcceptCommand
            => (new RelayCommand(Accept));

        public event EventHandler Closing;


        #endregion

        private void AddUser()
        {
            Users.Add(new SettingsListBoxItemViewModel(String.Empty));
        }

        private Boolean CanRemoveUser()
            => (SelectedUser != null);

        private void RemoveUser()
        {
            Users.Remove(SelectedUser);
        }

        private void AddRootFolder()
        {
            String rootFolder;
            if (UIServices.ShowFolderBrowserDialog(new FolderBrowserDialogOptions(), out rootFolder))
            {
                RootFolders.Add(rootFolder);
            }
        }

        private Boolean CanRemoveRootFolder()
            => (SelectedRootFolder != null);

        private void RemoveRootFolder()
        {
            RootFolders.Remove(SelectedRootFolder);
        }

        private void AddFileExtension()
        {
            FileExtensions.Add(new SettingsListBoxItemViewModel(String.Empty));
        }

        private Boolean CanRemoveFileExtension()
            => (SelectedFileExtension != null);

        private void RemoveFileExtension()
        {
            FileExtensions.Remove(SelectedFileExtension);
        }

        private void Accept()
        {
            HashSet<String> users = new HashSet<String>(Users.ForEach(item => item.Value));

            DataManager.Users = users.Where(item => String.IsNullOrEmpty(item) == false);

            HashSet<String> rootFolders = new HashSet<String>(RootFolders);

            DataManager.RootFolders = rootFolders.Where(item => String.IsNullOrEmpty(item) == false);

            HashSet<String> fileExtensions = new HashSet<String>(FileExtensions.ForEach(item => item.Value));

            DataManager.FileExtensions = fileExtensions.Where(item => String.IsNullOrEmpty(item) == false);

            Closing?.Invoke(this, EventArgs.Empty);
        }

        private void RaisePropertyChanged(String attribute)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
        }

    }
}