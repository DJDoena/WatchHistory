﻿using System;
using System.ComponentModel;
using System.Windows.Input;
using DoenaSoft.AbstractionLayer.Commands;
using DoenaSoft.AbstractionLayer.UIServices;
using DoenaSoft.WatchHistory.Implementations;

namespace DoenaSoft.WatchHistory.EditNote.Implementations
{
    internal sealed class EditNoteViewModel : IEditNoteViewModel
    {
        private string _note;

        public EditNoteViewModel(string note)
        {
            this.AcceptCommand = new RelayCommand(this.Accept);
            this.CancelCommand = new RelayCommand(this.Cancel);

            _note = note;
        }

        #region IEditTitleViewModel

        public ICommand AcceptCommand { get; }

        public ICommand CancelCommand { get; }

        public string Note
        {
            get => _note;
            set
            {
                if (_note != value)
                {
                    _note = value?.Trim();

                    this.RaisePropertyChanged(nameof(this.Note));
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
    }
}