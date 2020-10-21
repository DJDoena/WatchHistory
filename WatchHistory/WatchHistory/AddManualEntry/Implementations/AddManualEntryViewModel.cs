namespace DoenaSoft.WatchHistory.AddManualEntry.Implementations
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Text;
    using System.Windows.Input;
    using AbstractionLayer.IOServices;
    using AbstractionLayer.UIServices;
    using ToolBox.Commands;
    using WatchHistory.Data;
    using WatchHistory.Implementations;

    internal sealed class AddManualEntryViewModel : IAddManualEntryViewModel
    {
        private readonly IDataManager _dataManager;

        private readonly IIOServices _ioService;

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
            , IIOServices ioService
            , IUIServices uiServices
            , string userName)
        {
            _dataManager = dataManager;
            _ioService = ioService;
            _uiServices = uiServices;
            _userName = userName;

            AcceptCommand = new RelayCommand(Accept);
            CancelCommand = new RelayCommand(Cancel);

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

                    RaisePropertyChanged(nameof(WatchedDate));
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

                    RaisePropertyChanged(nameof(WatchedHour));
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

                    RaisePropertyChanged(nameof(WatchedMinute));
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

                    RaisePropertyChanged(nameof(LengthHours));
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

                    RaisePropertyChanged(nameof(LengthMinutes));
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

                    RaisePropertyChanged(nameof(LengthSeconds));
                }
            }
        }

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

        public string Note
        {
            get => _note;
            set
            {
                if (_note != value)
                {
                    _note = value;

                    RaisePropertyChanged(nameof(Note));
                }
            }
        }

        public event EventHandler<CloseEventArgs> Closing;

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private DateTime WatchedOn => new DateTime(WatchedDate.Year, WatchedDate.Month, WatchedDate.Day, WatchedHour, WatchedMinute, 0);

        private void Accept()
        {
            if (string.IsNullOrEmpty(Title))
            {
                _uiServices.ShowMessageBox("You need to enter a title", "Title Missing", Buttons.OK, Icon.Warning);

                return;
            }

            var folder = _ioService.Path.Combine(WatchHistory.Environment.AppDataFolder, "Manual");

            _ioService.Folder.CreateFolder(folder);

            var fileName = _ioService.Path.Combine(folder, $"{Guid.NewGuid()}.man");

            using (var fs = _ioService.GetFileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    sw.WriteLine(_note);
                }
            }

            var length = (uint)(new TimeSpan(LengthHours, LengthMinutes, LengthSeconds)).TotalSeconds;

            var watchedOn = WatchedOn;

            var entry = new FileEntry()
            {
                FullName = fileName,
                Title = Title,
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

            if (!string.IsNullOrWhiteSpace(_note))
            {
                entry.Note = _note;
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