namespace DoenaSoft.WatchHistory.EditTitle.Implementations
{
    using System;
    using System.ComponentModel;
    using System.Windows.Input;
    using AbstractionLayer.UIServices;
    using ToolBox.Commands;
    using WatchHistory.Implementations;

    internal sealed class EditTitleViewModel : IEditTitleViewModel
    {
        private string _title;

        public EditTitleViewModel(string title)
        {
            AcceptCommand = new RelayCommand(Accept);
            CancelCommand = new RelayCommand(Cancel);

            _title = title;
        }

        #region IEditTitleViewModel

        public ICommand AcceptCommand { get; }

        public ICommand CancelCommand { get; }

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;

                    RaisePropertyChanged(nameof(Title));
                }
            }
        }

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