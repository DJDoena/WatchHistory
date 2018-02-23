namespace DoenaSoft.WatchHistory.WatchedOn
{
    using System;
    using System.ComponentModel;
    using System.Windows.Input;

    internal interface IWatchedOnViewModel : INotifyPropertyChanged
    {
        ICommand AcceptCommand { get; }

        ICommand CancelCommand { get; }

        DateTime Value { get; set; }

        event EventHandler<CloseEventArgs> Closing;
    }
}