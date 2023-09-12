namespace DoenaSoft.WatchHistory.Settings.Implementations
{
    using System.ComponentModel;

    internal sealed class SettingsListBoxItemViewModel : ISettingsListBoxItemViewModel
    {
        private string _value;

        public SettingsListBoxItemViewModel(string value)
        {
            _value = value;
        }

        public string Value
        {
            get => _value;
            set
            {
                if (value != _value)
                {
                    _value = value;

                    this.RaisePropertyChanged(nameof(this.Value));
                }
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void RaisePropertyChanged(string attribute) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
    }
}