using System;
using System.ComponentModel;
using System.Windows.Input;
using DoenaSoft.AbstractionLayer.Commands;
using DoenaSoft.AbstractionLayer.IOServices;
using DoenaSoft.AbstractionLayer.UIServices;
using DoenaSoft.MediaInfoHelper.DataObjects;
using DoenaSoft.WatchHistory.Data;
using DoenaSoft.WatchHistory.Implementations;

namespace DoenaSoft.WatchHistory.AddYoutubeLink.Implementations
{
    internal sealed class AddYoutubeLinkViewModel : IAddYoutubeLinkViewModel
    {
        private readonly IYoutubeManager _youtubeManager;

        private readonly YoutubeFileManager _youtubeFileManager;

        private readonly IUIServices _uiServices;

        private readonly IClipboardServices _clipboardServices;

        private string _youtubeLink;

        private string _youtubeTitle;

        private DateTime _date;

        private byte _hour;

        private byte _minute;

        private YoutubeVideo _videoInfo;

        public AddYoutubeLinkViewModel(IDataManager dataManager, IIOServices ioServices, IUIServices uiServices, IClipboardServices clipboardServices, IYoutubeManager youtubeManager, string userName)
        {
            _youtubeManager = youtubeManager;
            _uiServices = uiServices;
            _clipboardServices = clipboardServices;

            _youtubeFileManager = new YoutubeFileManager(dataManager, ioServices, userName);
            this.AcceptCommand = new RelayCommand(this.Accept);
            this.CancelCommand = new RelayCommand(this.Cancel);
            this.ScanCommand = new RelayCommand(this.Scan);

            var now = DateTime.Now;

            _date = now.Date;

            _hour = (byte)(now.Hour);

            _minute = (byte)(now.Minute);
        }

        #region IAddYoutubeLinkViewModel

        public ICommand AcceptCommand { get; }

        public ICommand CancelCommand { get; }

        public ICommand ScanCommand { get; }

        public string YoutubeLink
        {
            get => _youtubeLink;
            set
            {
                if (_youtubeLink != value)
                {
                    _youtubeLink = value;

                    this.RaisePropertyChanged(nameof(this.YoutubeLink));
                }
            }
        }

        public string YoutubeTitle
        {
            get => _youtubeTitle;
            set
            {
                if (_youtubeTitle != value)
                {
                    _youtubeTitle = value;

                    this.RaisePropertyChanged(nameof(this.YoutubeTitle));
                }
            }
        }

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

        public event EventHandler<CloseEventArgs> Closing;

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private DateTime WatchedOn => new DateTime(this.Date.Year, this.Date.Month, this.Date.Day, this.Hour, this.Minute, 0);

        private void Accept()
        {
            if (_videoInfo == null && !this.GetYoutubeInfo())
            {
                return;
            }

            if (!string.IsNullOrEmpty(this.YoutubeTitle))
            {
                _videoInfo.Title = this.YoutubeTitle; //user input wins
            }

            if (string.IsNullOrEmpty(_videoInfo.Title))
            {
                _uiServices.ShowMessageBox("You need to enter a title", "Title Missing", Buttons.OK, Icon.Warning);

                return;
            }

            _youtubeFileManager.Add(_videoInfo, this.WatchedOn);

            Closing?.Invoke(this, new CloseEventArgs(Result.OK));
        }

        private void Cancel() => Closing?.Invoke(this, new CloseEventArgs(Result.Cancel));

        private void RaisePropertyChanged(string attribute) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));

        private void Scan()
        {
            this.YoutubeTitle = string.Empty;

            if (this.GetYoutubeInfo())
            {
                this.YoutubeTitle = _videoInfo.Title;
            }
        }

        private bool GetYoutubeInfo()
        {
            _videoInfo = null;

            if (string.IsNullOrEmpty(this.YoutubeLink) && _clipboardServices.ContainsText)
            {
                this.TryGetLinkFromClipboard();
            }

            if (string.IsNullOrEmpty(this.YoutubeLink))
            {
                _uiServices.ShowMessageBox("You need to enter a valid Youtube URL", "Missing URL", Buttons.OK, Icon.Warning);
            }

            try
            {
                _videoInfo = _youtubeManager.GetInfo(this.YoutubeLink);
            }
            catch (YoutubeUrlException ex)
            {
                _uiServices.ShowMessageBox(ex.Message, "Error", Buttons.OK, Icon.Error);
            }
            catch (Exception ex)
            {
                _uiServices.ShowMessageBox(ex.Message, "Unexpected Error", Buttons.OK, Icon.Error);
            }

            return _videoInfo != null;
        }

        private void TryGetLinkFromClipboard()
        {
            try
            {
                this.YoutubeLink = _clipboardServices.GetText();
            }
            catch
            { }
        }
    }
}