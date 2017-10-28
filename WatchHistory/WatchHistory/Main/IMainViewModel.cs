using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using DoenaSoft.WatchHistory.Data;

namespace DoenaSoft.WatchHistory.Main
{
    internal interface IMainViewModel : INotifyPropertyChanged
    {
        String Title { get; }

        String Filter { get; set; }

        Boolean IgnoreWatched { get; set; }

        ObservableCollection<IFileEntryViewModel> Entries { get; }

        ICommand AddWatchedCommand { get; }

        ICommand IgnoreCommand { get; }

        ICommand OpenSettingsCommand { get; }

        ICommand ImportCollectionCommand { get; }

        ICommand UndoIgnoreCommand { get; }
    }
}