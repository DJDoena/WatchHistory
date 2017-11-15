namespace DoenaSoft.WatchHistory.Settings
{
    using System;
    using System.ComponentModel;

    internal interface ISettingsListBoxItemViewModel : INotifyPropertyChanged
    {
        String Value { get; set; }
    }
}