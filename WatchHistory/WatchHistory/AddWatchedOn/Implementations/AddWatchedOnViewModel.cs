using System;
using System.ComponentModel;
using System.Windows.Input;
using DoenaSoft.AbstractionLayer.Commands;
using DoenaSoft.AbstractionLayer.UIServices;
using DoenaSoft.WatchHistory.Implementations;

namespace DoenaSoft.WatchHistory.AddWatchedOn.Implementations
{
    internal sealed class AddWatchedOnViewModel : IAddWatchedOnViewModel
    {
        private static DateTime _date;

        private static byte _hour;

        private static byte _minute;

        static AddWatchedOnViewModel()
        {
            var now = DateTime.Now.AddMinutes(-1);

            _date = now.Date;

            _hour = (byte)now.Hour;

            _minute = (byte)now.Minute;
        }

        public AddWatchedOnViewModel()
        {
            this.AcceptCommand = new RelayCommand(this.Accept);
            this.CancelCommand = new RelayCommand(this.Cancel);

            var lastWatched = new DateTime(_date.Year, _date.Month, this.Date.Day, _hour, _minute, 0);

            if (lastWatched != new DateTime(2000, 1, 1, 0, 0, 0))
            {
                lastWatched = lastWatched.AddMinutes(1);

                _date = lastWatched.Date;

                _hour = (byte)lastWatched.Hour;

                _minute = (byte)lastWatched.Minute;
            }
        }

        #region IAddWatchedOnViewModel

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

                    this.RaisePropertyChanged(nameof(this.Date));
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

                    this.RaisePropertyChanged(nameof(this.Hour));
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

                    this.RaisePropertyChanged(nameof(this.Minute));
                }
            }
        }

        public DateTime WatchedOn => new DateTime(this.Date.Year, this.Date.Month, this.Date.Day, this.Hour, this.Minute, 0);

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