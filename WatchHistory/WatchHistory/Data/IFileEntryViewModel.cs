namespace DoenaSoft.WatchHistory.Data
{
    using System;
    using System.ComponentModel;
    using System.Windows.Media;

    internal interface IFileEntryViewModel : INotifyPropertyChanged
    {
        FileEntry FileEntry { get; }

        String Name { get; }

        String LastWatched { get; }

        String CreationTime { get; }

        String RunningTime { get; }

        Brush Color { get; }
    }
}