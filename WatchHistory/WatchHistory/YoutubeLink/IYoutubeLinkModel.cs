namespace DoenaSoft.WatchHistory.YoutubeLink
{
    using System;
    using System.ComponentModel;
    using System.Windows.Input;
    using WatchHistory.Implementations;

    internal interface IYoutubeLinkViewModel : INotifyPropertyChanged
    {
        ICommand AcceptCommand { get; }

        ICommand CancelCommand { get; }

        string YoutubeLink { get; set; }

        ICommand ScanCommand { get; }

        string YoutubeTitle { get; set; }

        DateTime Date { get; set; }

        byte Hour { get; set; }

        byte Minute { get; set; }

        event EventHandler<CloseEventArgs> Closing;
    }
}