using System;
using System.ComponentModel;

namespace DoenaSoft.WatchHistory.Settings
{
    internal interface ISettingsListBoxItemViewModel : INotifyPropertyChanged
    {
        String Value { get; set; }
    }
}