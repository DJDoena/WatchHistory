namespace DoenaSoft.WatchHistory.Ignore
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows.Input;
    using Data;

    internal interface IIgnoreViewModel : INotifyPropertyChanged
    {
        string Filter { get; set; }

        bool SearchInPath { get; set; }

        ObservableCollection<IFileEntryViewModel> Entries { get; }

        ICommand UndoIgnoreCommand { get; }
    }
}