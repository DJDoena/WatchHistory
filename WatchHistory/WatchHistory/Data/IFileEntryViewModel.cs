namespace DoenaSoft.WatchHistory.Data
{
    using System.ComponentModel;
    using System.Windows.Media;

    internal interface IFileEntryViewModel : INotifyPropertyChanged
    {
        FileEntry FileEntry { get; }

        string Name { get; }

        string LastWatched { get; }

        string CreationTime { get; }

        string RunningTime { get; }

        Brush Color { get; }
    }
}