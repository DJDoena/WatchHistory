namespace DoenaSoft.WatchHistory.RunningTime.Implementations
{
    using System;
    using System.ComponentModel;
    using System.Windows.Input;
    using AbstractionLayer.UIServices;
    using ToolBox.Commands;
    using WatchHistory.Implementations;

    internal sealed class RunningTimeViewModel : IRunningTimeViewModel
    {
        private Byte _Hours;

        private Byte _Minutes;

        private Byte _Seconds;

        public RunningTimeViewModel(UInt32 seconds)
        {
            AcceptCommand = new RelayCommand(Accept);
            CancelCommand = new RelayCommand(Cancel);

            _Hours = (Byte)(seconds / 3600);

            UInt32 modulo = seconds % 3600;

            _Minutes = (Byte)(modulo / 60);

            _Seconds = (Byte)(modulo % 60);
        }

        #region IRunningTimeViewModel

        public ICommand AcceptCommand { get; }

        public ICommand CancelCommand { get; }

        public Byte Hours
        {
            get => _Hours;
            set
            {
                if (_Hours != value)
                {
                    _Hours = value;

                    RaisePropertyChanged(nameof(Hours));
                }
            }
        }

        public Byte Minutes
        {
            get => _Minutes;
            set
            {
                if (_Minutes != value)
                {
                    _Minutes = value;

                    RaisePropertyChanged(nameof(Minutes));
                }
            }
        }

        public Byte Seconds
        {
            get => _Seconds;
            set
            {
                if (_Seconds != value)
                {
                    _Seconds = value;

                    RaisePropertyChanged(nameof(Seconds));
                }
            }
        }

        public UInt32 RunningTime
            => (UInt32)(Hours * 3600 + Minutes * 60 + Seconds);

        public event EventHandler<CloseEventArgs> Closing;

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void Accept()
            => Closing?.Invoke(this, new CloseEventArgs(Result.OK));

        private void Cancel()
            => Closing?.Invoke(this, new CloseEventArgs(Result.Cancel));

        private void RaisePropertyChanged(String attribute)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
    }
}