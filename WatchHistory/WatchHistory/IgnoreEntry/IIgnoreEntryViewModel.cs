namespace DoenaSoft.WatchHistory.IgnoreEntry
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Input;
    using Data;

    internal interface IIgnoreEntryViewModel : INotifyPropertyChanged
    {
        string Filter { get; set; }

        bool SearchInPath { get; set; }

        ObservableCollection<IFileEntryViewModel> Entries { get; }

        ICommand UndoIgnoreCommand { get; }
    }
}