namespace DoenaSoft.WatchHistory.AddManualEntry
{
    using System;
    using System.ComponentModel;
    using System.Windows.Input;
    using WatchHistory.Implementations;

    internal interface IAddManualEntryViewModel : INotifyPropertyChanged
    {
        ICommand AcceptCommand { get; }

        ICommand CancelCommand { get; }

        DateTime WatchedDate { get; set; }

        byte WatchedHour { get; set; }

        byte WatchedMinute { get; set; }

        byte LengthHours { get; set; }

        byte LengthMinutes { get; set; }

        byte LengthSeconds { get; set; }

        string Title { get; set; }

        string Note { get; set; }

        event EventHandler<CloseEventArgs> Closing;
    }
}