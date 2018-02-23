namespace DoenaSoft.WatchHistory.Main
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Input;
    using Data;

    internal interface IMainViewModel : INotifyPropertyChanged
    {
        String Title { get; }

        String Filter { get; set; }

        Boolean IgnoreWatched { get; set; }

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
    }
}