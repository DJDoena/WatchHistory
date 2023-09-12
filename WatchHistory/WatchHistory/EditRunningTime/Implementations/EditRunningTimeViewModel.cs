using System;
using System.ComponentModel;
using System.Windows.Input;
using DoenaSoft.AbstractionLayer.Commands;
using DoenaSoft.AbstractionLayer.UIServices;
using DoenaSoft.WatchHistory.Implementations;

namespace DoenaSoft.WatchHistory.EditRunningTime.Implementations
{
    internal sealed class EditRunningTimeViewModel : IEditRunningTimeViewModel
    {
        private byte _hours;

        private byte _minutes;

        private byte _seconds;

        public EditRunningTimeViewModel(uint seconds)
        {
            this.AcceptCommand = new RelayCommand(this.Accept);
            this.CancelCommand = new RelayCommand(this.Cancel);

            _hours = (byte)(seconds / 3600);

            var modulo = seconds % 3600;

            _minutes = (byte)(modulo / 60);

            _seconds = (byte)(modulo % 60);
        }

        #region IEditRunningTimeViewModel

        public ICommand AcceptCommand { get; }

        public ICommand CancelCommand { get; }

        public byte Hours
        {
            get => _hours;
            set
            {
                if (_hours != value)
                {
                    _hours = value;

                    this.RaisePropertyChanged(nameof(this.Hours));
                }
            }
        }

        public byte Minutes
        {
            get => _minutes;
            set
            {
                if (_minutes != value)
                {
                    _minutes = value;

                    this.RaisePropertyChanged(nameof(this.Minutes));
                }
            }
        }

        public byte Seconds
        {
            get => _seconds;
            set
            {
                if (_seconds != value)
                {
                    _seconds = value;

                    this.RaisePropertyChanged(nameof(this.Seconds));
                }
            }
        }

        public uint RunningTime => (uint)(new TimeSpan(this.Hours, this.Minutes, this.Seconds)).TotalSeconds;

        public event EventHandler<CloseEventArgs> Closing;

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void Accept() => Closing?.Invoke(this, new CloseEventArgs(Result.OK));

        private void Cancel() => Closing?.Invoke(this, new CloseEventArgs(Result.Cancel));

        private void RaisePropertyChanged(string attribute) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
    }
}