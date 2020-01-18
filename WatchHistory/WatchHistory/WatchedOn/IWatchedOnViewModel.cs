namespace DoenaSoft.WatchHistory.WatchedOn
{
    using System;
    using System.ComponentModel;
    using System.Windows.Input;
    using WatchHistory.Implementations;

    internal interface IWatchedOnViewModel : INotifyPropertyChanged
    {
        ICommand AcceptCommand { get; }

        ICommand CancelCommand { get; }

        DateTime Date { get; set; }

        byte Hour { get; set; }

        byte Minute { get; set; }

        DateTime WatchedOn { get; }

        event EventHandler<CloseEventArgs> Closing;
    }
}