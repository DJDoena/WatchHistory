namespace DoenaSoft.WatchHistory.EditTitle
{
    using System;
    using System.ComponentModel;
    using System.Windows.Input;
    using WatchHistory.Implementations;

    internal interface IEditTitleViewModel : INotifyPropertyChanged
    {
        ICommand AcceptCommand { get; }

        ICommand CancelCommand { get; }

        string Title { get; set; }

        event EventHandler<CloseEventArgs> Closing;
    }
}