namespace DoenaSoft.WatchHistory.Settings.Implementations
{
    using System;
    using System.ComponentModel;

    internal sealed class SettingsListBoxItemViewModel : ISettingsListBoxItemViewModel
    {
        private String _Value;

        public SettingsListBoxItemViewModel(String value)
        {
            _Value = value;
        }

        public String Value
        {
            get => _Value;
            set
            {
                if (value != _Value)
                {
                    _Value = value;

                    RaisePropertyChanged(nameof(Value));
                }
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void RaisePropertyChanged(String attribute)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
        }
    }
}