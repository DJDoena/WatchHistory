namespace DoenaSoft.WatchHistory.WatchedOn
{
    using System;
    using System.ComponentModel;
    using System.Windows.Input;

    internal interface IWatchedOnViewModel : INotifyPropertyChanged
    {
        ICommand AcceptCommand { get; }

        ICommand CancelCommand { get; }

        DateTime Date { get; set; }

        Byte Hour { get; set; }

        Byte Minute { get; set; }

        DateTime WatchedOn { get; }

        event EventHandler<CloseEventArgs> Closing;
    }
}