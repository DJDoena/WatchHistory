namespace DoenaSoft.WatchHistory.Settings
{
    using System.ComponentModel;

    internal interface ISettingsListBoxItemViewModel : INotifyPropertyChanged
    {
        string Value { get; set; }
    }
}