using System;
using System.ComponentModel;
using System.Windows.Media;

namespace DoenaSoft.WatchHistory.Data
{
    internal interface IFileEntryViewModel : INotifyPropertyChanged
    {
        FileEntry FileEntry { get; }

        String Name { get; }

        String LastWatched { get; }

        Brush Color { get; }
    }
}