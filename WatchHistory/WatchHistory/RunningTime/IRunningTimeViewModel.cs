namespace DoenaSoft.WatchHistory.RunningTime
{
    using System;
    using System.ComponentModel;
    using System.Windows.Input;
    using WatchHistory.Implementations;

    internal interface IRunningTimeViewModel : INotifyPropertyChanged
    {
        ICommand AcceptCommand { get; }

        ICommand CancelCommand { get; }

        byte Hours { get; set; }

        byte Minutes { get; set; }

        byte Seconds { get; set; }

        uint RunningTime { get; }

        event EventHandler<CloseEventArgs> Closing;
    }
}