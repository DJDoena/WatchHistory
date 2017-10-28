using System;
using System.ComponentModel;

namespace DoenaSoft.WatchHistory.Settings.Implementations
{
    internal sealed class SettingsListBoxItemViewModel : ISettingsListBoxItemViewModel
    {
        private String m_Value;

        public SettingsListBoxItemViewModel(String value)
        {
            m_Value = value;
        }

        public String Value
        {
            get
            {
                return (m_Value);
            }

            set
            {
                if(value != m_Value)
                {
                    m_Value = value;

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