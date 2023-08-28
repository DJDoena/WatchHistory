using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using DoenaSoft.AbstractionLayer.Commands;
using DoenaSoft.AbstractionLayer.UIServices;
using DoenaSoft.WatchHistory.Data;
using DoenaSoft.WatchHistory.Implementations;

namespace DoenaSoft.WatchHistory.EditTitle.Implementations
{
    internal sealed class EditTitleViewModel : IEditTitleViewModel
    {
        private string _title;

        public EditTitleViewModel(FileEntry entry)
        {
            AcceptCommand = new RelayCommand(Accept);
            CancelCommand = new RelayCommand(Cancel);

            _title = GetCurrentTitle(entry);
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

        private void Accept() => Closing?.Invoke(this, new CloseEventArgs(Result.OK));

        private void Cancel() => Closing?.Invoke(this, new CloseEventArgs(Result.Cancel));

        private void RaisePropertyChanged(string attribute) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));

        private static string GetCurrentTitle(FileEntry entry)
            => !string.IsNullOrWhiteSpace(entry.Title)
                ? entry.Title
                : Path.GetFileNameWithoutExtension(entry.FullName);
    }
}