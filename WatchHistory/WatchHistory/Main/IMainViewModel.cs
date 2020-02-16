namespace DoenaSoft.WatchHistory.Main
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Input;
    using Data;

    internal interface IMainViewModel : INotifyPropertyChanged
    {
        string Title { get; }

        string Filter { get; set; }

        bool IgnoreWatched { get; set; }

        bool SearchInPath { get; set; }

        ObservableCollection<IFileEntryViewModel> Entries { get; }

        ICommand AddWatchedCommand { get; }

        ICommand PlayFileAndAddWatchedCommand { get; }

        ICommand IgnoreCommand { get; }

        ICommand OpenSettingsCommand { get; }

        ICommand ImportCollectionCommand { get; }

        ICommand UndoIgnoreCommand { get; }

        ICommand PlayFileCommand { get; }

        ICommand SortCommand { get; }

        ICommand OpenFileLocationCommand { get; }

        ICommand AddWatchedOnCommand { get; }

        ICommand CheckForUpdateCommand { get; }

        ICommand AboutCommand { get; }

        ICommand ShowHistoryCommand { get; }

        ICommand EditRunningTimeCommand { get; }

        ICommand AddYoutubeLinkCommand { get; }

        ICommand AddManualEntryCommand { get; }
    }
}