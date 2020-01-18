﻿namespace DoenaSoft.WatchHistory.RunningTime.Implementations
{
    using System;
    using System.ComponentModel;
    using System.Windows.Input;
    using AbstractionLayer.UIServices;
    using ToolBox.Commands;
    using WatchHistory.Implementations;

    internal sealed class RunningTimeViewModel : IRunningTimeViewModel
    {
        private byte _hours;

        private byte _minutes;

        private byte _seconds;

        public RunningTimeViewModel(uint seconds)
        {
            AcceptCommand = new RelayCommand(Accept);
            CancelCommand = new RelayCommand(Cancel);

            _hours = (byte)(seconds / 3600);

            var modulo = seconds % 3600;

            _minutes = (byte)(modulo / 60);

            _seconds = (byte)(modulo % 60);
        }

        #region IRunningTimeViewModel

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

                    RaisePropertyChanged(nameof(Hours));
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

                    RaisePropertyChanged(nameof(Minutes));
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

                    RaisePropertyChanged(nameof(Seconds));
                }
            }
        }

        public uint RunningTime
            => (uint)(Hours * 3600 + Minutes * 60 + Seconds);

        public event EventHandler<CloseEventArgs> Closing;

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void Accept()
            => Closing?.Invoke(this, new CloseEventArgs(Result.OK));

        private void Cancel()
            => Closing?.Invoke(this, new CloseEventArgs(Result.Cancel));

        private void RaisePropertyChanged(string attribute)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
    }
}