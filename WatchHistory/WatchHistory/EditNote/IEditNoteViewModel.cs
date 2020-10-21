namespace DoenaSoft.WatchHistory.EditNote
{
    using System;
    using System.ComponentModel;
    using System.Windows.Input;
    using WatchHistory.Implementations;

    internal interface IEditNoteViewModel : INotifyPropertyChanged
    {
        ICommand AcceptCommand { get; }

        ICommand CancelCommand { get; }

        string Note { get; set; }

        event EventHandler<CloseEventArgs> Closing;
    }
}