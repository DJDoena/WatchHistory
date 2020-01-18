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
        private DateTime _date;

        private byte _hour;

        private byte _minute;

        public WatchedOnViewModel()
        {
            AcceptCommand = new RelayCommand(Accept);
            CancelCommand = new RelayCommand(Cancel);

            var now = DateTime.Now;

            _date = now.Date;

            _hour = (byte)(now.Hour);

            _minute = (byte)(now.Minute);
        }

        #region IWatchedOnViewModel

        public ICommand AcceptCommand { get; }

        public ICommand CancelCommand { get; }

        public DateTime Date
        {
            get => _date;
            set
            {
                if (_date != value)
                {
                    _date = value;

                    RaisePropertyChanged(nameof(Date));
                }
            }
        }

        public byte Hour
        {
            get => _hour;
            set
            {
                if (_hour != value)
                {
                    _hour = value;

                    RaisePropertyChanged(nameof(Hour));
                }
            }
        }

        public byte Minute
        {
            get => _minute;
            set
            {
                if (_minute != value)
                {
                    _minute = value;

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

        private void RaisePropertyChanged(string attribute)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
    }
}