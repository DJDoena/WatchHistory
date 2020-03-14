namespace DoenaSoft.WatchHistory.ShowReport
{
    using System;
    using System.ComponentModel;
    using System.Windows.Input;
    using WatchHistory.Implementations;

    internal interface IShowReportViewModel : INotifyPropertyChanged
    {
        ICommand ReportDayCommand { get; }

        ICommand ReportMonthCommand { get; }

        ICommand CancelCommand { get; }

        DateTime Date { get; set; }

        event EventHandler<CloseEventArgs> Closing;
    }
}