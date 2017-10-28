using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using DoenaSoft.WatchHistory.Data;

namespace DoenaSoft.WatchHistory.Ignore
{
    internal interface IIgnoreViewModel : INotifyPropertyChanged
    {
        String Title { get; }

        String Filter { get; set; }

        ObservableCollection<IFileEntryViewModel> Entries { get; }

        ICommand UndoIgnoreCommand { get; }
    }
}