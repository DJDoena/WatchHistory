namespace DoenaSoft.WatchHistory.SelectUser
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Input;

    internal interface ISelectUserViewModel : INotifyPropertyChanged
    {
        IEnumerable<String> Users { get; }

        String SelectedUser { get; set; }

        ICommand SelectCommand { get; }

        event EventHandler Closing;
    }
}