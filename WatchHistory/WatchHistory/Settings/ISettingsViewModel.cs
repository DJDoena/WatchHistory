namespace DoenaSoft.WatchHistory.Settings
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Input;

    internal interface ISettingsViewModel : INotifyPropertyChanged
    {
        ObservableCollection<ISettingsListBoxItemViewModel> Users { get; }

        ISettingsListBoxItemViewModel SelectedUser { get; set; }

        ICommand AddUserCommand { get; }

        ICommand RemoveUserCommand { get; }

        ObservableCollection<String> RootFolders { get; }

        String SelectedRootFolder { get; set; }

        ICommand AddRootFolderCommand { get; }

        ICommand RemoveRootFolderCommand { get; }

        ObservableCollection<ISettingsListBoxItemViewModel> FileExtensions { get; }

        ISettingsListBoxItemViewModel SelectedFileExtension { get; set; }

        ICommand AddFileExtensionCommand { get; }

        ICommand RemoveFileExtensionCommand { get; }

        ICommand AcceptCommand { get; }

        event EventHandler Closing;
    }
}