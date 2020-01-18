namespace DoenaSoft.WatchHistory.SelectUser
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Input;

    internal interface ISelectUserViewModel : INotifyPropertyChanged
    {
        IEnumerable<string> Users { get; }

        string SelectedUser { get; set; }

        ICommand SelectCommand { get; }

        event EventHandler Closing;
    }
}