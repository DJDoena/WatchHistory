namespace DoenaSoft.WatchHistory.Ignore
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Input;
    using Data;

    internal interface IIgnoreViewModel : INotifyPropertyChanged
    {
        String Title { get; }

        String Filter { get; set; }

        ObservableCollection<IFileEntryViewModel> Entries { get; }

        ICommand UndoIgnoreCommand { get; }
    }
}