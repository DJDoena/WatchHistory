namespace DoenaSoft.WatchHistory.WatchedOn.Implementations
{
    using System;
    using System.ComponentModel;
    using System.Windows.Input;
    using AbstractionLayer.UIServices;
    using ToolBox.Commands;
    using WatchHistory.Implementations;

    internal sealed class WatchedOnViewModel : IWatchedOnViewModel
    {
        private readonly ICommand _AcceptCommand;

        private readonly ICommand _CancelCommand;

        private DateTime _Date;

        private Byte _Hour;

        private Byte _Minute;

        public WatchedOnViewModel()
        {
            _AcceptCommand = new RelayCommand(Accept);
            _CancelCommand = new RelayCommand(Cancel);

            DateTime now = DateTime.Now;

            _Date = now.Date;

            _Hour = (Byte)(now.Hour);

            _Minute = (Byte)(now.Minute);
        }

        #region IWatchedOnViewModel

        public ICommand AcceptCommand
            => _AcceptCommand;

        public ICommand CancelCommand
            => _CancelCommand;

        public DateTime Date
        {
            get => _Date;
            set
            {
                if (_Date != value)
                {
                    _Date = value;

                    RaisePropertyChanged(nameof(Date));
                }
            }
        }

        public Byte Hour
        {
            get => _Hour;
            set
            {
                if (_Hour != value)
                {
                    _Hour = value;

                    RaisePropertyChanged(nameof(Hour));
                }
            }
        }

        public Byte Minute
        {
            get => _Minute;
            set
            {
                if (_Minute != value)
                {
                    _Minute = value;

                    RaisePropertyChanged(nameof(Minute));
                }
            }
        }

        public DateTime WatchedOn
            => new DateTime(Date.Year, Date.Month, Date.Day, Hour, Minute, 0);

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