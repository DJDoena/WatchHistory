﻿namespace DoenaSoft.WatchHistory.AddManualEntry.Implementations
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows.Input;
    using AbstractionLayer.IOServices;
    using AbstractionLayer.UIServices;
    using MediaInfoHelper;
    using ToolBox.Commands;
    using ToolBox.Extensions;
    using WatchHistory.Data;
    using WatchHistory.Implementations;

    internal sealed class AddManualEntryViewModel : IAddManualEntryViewModel
    {
        private readonly IDataManager _dataManager;

        private readonly IIOServices _ioServices;

        private readonly IUIServices _uiServices;

        private readonly string _userName;

        private DateTime _watchedDate;

        private byte _watchedHour;

        private byte _watchedMinute;

        private byte _lengtHours;

        private byte _lengthMinutes;

        private byte _lengthSeconds;

        private string _title;

        private string _note;

        public AddManualEntryViewModel(IDataManager dataManager
            , IIOServices ioServices
            , IUIServices uiServices
            , string userName)
        {
            _dataManager = dataManager;
            _ioServices = ioServices;
            _uiServices = uiServices;
            _userName = userName;

            this.AcceptCommand = new RelayCommand(this.Accept);
            this.CancelCommand = new RelayCommand(this.Cancel);

            var now = DateTime.Now;

            _watchedDate = now.Date;

            _watchedHour = (byte)(now.Hour);

            _watchedMinute = (byte)(now.Minute);
        }

        #region IAddManualEntryViewModel

        public ICommand AcceptCommand { get; }

        public ICommand CancelCommand { get; }

        public DateTime WatchedDate
        {
            get => _watchedDate;
            set
            {
                if (_watchedDate != value)
                {
                    _watchedDate = value;

                    this.RaisePropertyChanged(nameof(this.WatchedDate));
                }
            }
        }

        public byte WatchedHour
        {
            get => _watchedHour;
            set
            {
                if (_watchedHour != value)
                {
                    _watchedHour = value;

                    this.RaisePropertyChanged(nameof(this.WatchedHour));
                }
            }
        }

        public byte WatchedMinute
        {
            get => _watchedMinute;
            set
            {
                if (_watchedMinute != value)
                {
                    _watchedMinute = value;

                    this.RaisePropertyChanged(nameof(this.WatchedMinute));
                }
            }
        }

        public byte LengthHours
        {
            get => _lengtHours;
            set
            {
                if (_lengtHours != value)
                {
                    _lengtHours = value;

                    this.RaisePropertyChanged(nameof(this.LengthHours));
                }
            }
        }

        public byte LengthMinutes
        {
            get => _lengthMinutes;
            set
            {
                if (_lengthMinutes != value)
                {
                    _lengthMinutes = value;

                    this.RaisePropertyChanged(nameof(this.LengthMinutes));
                }
            }
        }

        public byte LengthSeconds
        {
            get => _lengthSeconds;
            set
            {
                if (_lengthSeconds != value)
                {
                    _lengthSeconds = value;

                    this.RaisePropertyChanged(nameof(this.LengthSeconds));
                }
            }
        }

        public string Title
        {
            get => _title ?? string.Empty;
            set
            {
                if (_title != value)
                {
                    _title = value;

                    this.RaisePropertyChanged(nameof(this.Title));
                }
            }
        }

        public string Note
        {
            get => _note ?? string.Empty;
            set
            {
                if (_note != value)
                {
                    _note = value;

                    this.RaisePropertyChanged(nameof(this.Note));
                }
            }
        }

        public event EventHandler<CloseEventArgs> Closing;

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private DateTime WatchedOn => new DateTime(this.WatchedDate.Year, this.WatchedDate.Month, this.WatchedDate.Day, this.WatchedHour, this.WatchedMinute, 0);

        private void Accept()
        {
            if (string.IsNullOrWhiteSpace(this.Title))
            {
                _uiServices.ShowMessageBox("You need to enter a title", "Title Missing", Buttons.OK, Icon.Warning);

                return;
            }

            var folder = _ioServices.Path.Combine(WatchHistory.Environment.MyDocumentsFolder, "Manual");

            _ioServices.Folder.CreateFolder(folder);

            _dataManager.RootFolders = folder.Enumerate().Union(_dataManager.RootFolders);

            _dataManager.FileExtensions = Constants.ManualFileExtensionName.Enumerate().Union(_dataManager.FileExtensions);

            var fileName = _ioServices.Path.Combine(folder, $"{Guid.NewGuid()}.{Constants.ManualFileExtensionName}");

            using (var fs = _ioServices.GetFileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    sw.WriteLine(_note?.Trim());
                }
            }

            var watchedOn = this.WatchedOn.ToUniversalTime().Conform();

            var fi = _ioServices.GetFileInfo(fileName);

            fi.CreationTimeUtc = watchedOn;

            var length = (uint)(new TimeSpan(this.LengthHours, this.LengthMinutes, this.LengthSeconds)).TotalSeconds;

            var entry = new FileEntry()
            {
                FullName = fileName,
                Title = this.Title.Trim(),
                VideoLength = length,
                CreationTime = watchedOn,
                Users = new User[]
                {
                    new User()
                    {
                        UserName = _userName,
                        Watches = new Watch[]
                        {
                            new Watch()
                            {
                                 Value = watchedOn,
                            },
                        },
                    },
                },
            };

            if (!string.IsNullOrWhiteSpace(this.Note))
            {
                entry.Note = this.Note.Trim();
            }

            _dataManager.TryCreateEntry(entry);

            Closing?.Invoke(this, new CloseEventArgs(Result.OK));
        }

        private void Cancel()
            => Closing?.Invoke(this, new CloseEventArgs(Result.Cancel));

        private void RaisePropertyChanged(string attribute)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
    }
}