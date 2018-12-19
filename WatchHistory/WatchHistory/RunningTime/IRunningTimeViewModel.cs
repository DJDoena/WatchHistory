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

        Byte Hours { get; set; }

        Byte Minutes { get; set; }

        Byte Seconds { get; set; }

        UInt32 RunningTime { get; }

        event EventHandler<CloseEventArgs> Closing;
    }
}