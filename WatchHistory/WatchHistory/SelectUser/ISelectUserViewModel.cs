using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace DoenaSoft.WatchHistory.SelectUser
{
    internal interface ISelectUserViewModel : INotifyPropertyChanged
    {
        IEnumerable<String> Users { get; }

        String SelectedUser { get; set; }

        ICommand SelectCommand { get; }

        event EventHandler Closing;
    }
}