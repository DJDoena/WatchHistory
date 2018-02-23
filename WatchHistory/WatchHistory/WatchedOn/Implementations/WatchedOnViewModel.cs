namespace DoenaSoft.WatchHistory.WatchedOn.Implementations
{
    using System;
    using System.ComponentModel;
    using System.Windows.Input;
    using AbstractionLayer.UIServices;
    using ToolBox.Commands;

    internal sealed class WatchedOnViewModel : IWatchedOnViewModel
    {
        private readonly ICommand _AcceptCommand;

        private readonly ICommand _CancelCommand;

        private DateTime _Value;

        public WatchedOnViewModel()
        {            
            _AcceptCommand = new RelayCommand(Accept);
            _CancelCommand = new RelayCommand(Cancel);

            _Value = DateTime.Now;
        }

        #region IMainViewModel

        public ICommand AcceptCommand 
            => _AcceptCommand;

        public ICommand CancelCommand 
            => _CancelCommand;

        public DateTime Value
        {
            get => _Value;
            set
            {
                if(_Value != value)
                {
                    _Value = value;

                    RaisePropertyChanged(nameof(Value));
                }
            }
        }

        public event EventHandler<CloseEventArgs> Closing;

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void Accept()
        {
            Closing?.Invoke(this, new CloseEventArgs(Result.OK));
        }

        private void Cancel()
        {
            Closing?.Invoke(this, new CloseEventArgs(Result.Cancel));
        }

        private void RaisePropertyChanged(String attribute)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
        }
    }
}