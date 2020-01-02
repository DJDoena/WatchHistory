namespace DoenaSoft.WatchHistory.YoutubeLink.Implementations
{
    using System;
    using System.ComponentModel;
    using System.Windows.Input;
    using AbstractionLayer.IOServices;
    using AbstractionLayer.UIServices;
    using DoenaSoft.MediaInfoHelper.Youtube;
    using ToolBox.Commands;
    using WatchHistory.Data;
    using WatchHistory.Implementations;

    internal sealed class YoutubeLinkViewModel : IYoutubeLinkViewModel
    {
        private readonly IYoutubeManager _youtubeManager;

        private readonly YoutubeFileManager _youtubeFileManager;

        private readonly IUIServices _uiServices;

        private readonly string _userName;

        private string _YoutubeLink;

        private string _YoutubeTitle;

        private DateTime _Date;

        private byte _Hour;

        private byte _Minute;

        private YoutubeVideoInfo _videoInfo;

        public YoutubeLinkViewModel(IDataManager dataManager
            , IIOServices ioServices
            , IUIServices uiServices
            , IYoutubeManager youtubeManager
            , string userName)
        {
            _youtubeManager = youtubeManager;
            _uiServices = uiServices;
            _userName = userName;

            _youtubeFileManager = new YoutubeFileManager(dataManager, ioServices, userName);
            AcceptCommand = new RelayCommand(Accept);
            CancelCommand = new RelayCommand(Cancel);
            ScanCommand = new RelayCommand(Scan);

            DateTime now = DateTime.Now;

            _Date = now.Date;

            _Hour = (byte)(now.Hour);

            _Minute = (byte)(now.Minute);
        }

        #region IYoutubeLinkViewModel

        public ICommand AcceptCommand { get; }

        public ICommand CancelCommand { get; }

        public ICommand ScanCommand { get; }

        public string YoutubeLink
        {
            get => _YoutubeLink;
            set
            {
                if (_YoutubeLink != value)
                {
                    _YoutubeLink = value;

                    RaisePropertyChanged(nameof(YoutubeLink));
                }
            }
        }

        public string YoutubeTitle
        {
            get => _YoutubeTitle;
            set
            {
                if (_YoutubeTitle != value)
                {
                    _YoutubeTitle = value;

                    RaisePropertyChanged(nameof(YoutubeTitle));
                }
            }
        }

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

        public byte Hour
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

        public byte Minute
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

        public event EventHandler<CloseEventArgs> Closing;

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private DateTime WatchedOn
            => new DateTime(Date.Year, Date.Month, Date.Day, Hour, Minute, 0);

        private void Accept()
        {
            if (_videoInfo == null && !GetYoutubeInfo())
            {
                return;
            }

            if (!string.IsNullOrEmpty(YoutubeTitle))
            {
                _videoInfo.Title = YoutubeTitle; //user input wins
            }

            if (string.IsNullOrEmpty(_videoInfo.Title))
            {
                _uiServices.ShowMessageBox("You need to enter a title", "Title Missing", Buttons.OK, Icon.Warning);

                return;
            }

            _youtubeFileManager.Add(_videoInfo, WatchedOn);

            Closing?.Invoke(this, new CloseEventArgs(Result.OK));
        }

        private void Cancel()
            => Closing?.Invoke(this, new CloseEventArgs(Result.Cancel));

        private void RaisePropertyChanged(string attribute)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));

        private void Scan()
        {
            YoutubeTitle = string.Empty;

            if (GetYoutubeInfo())
            {
                YoutubeTitle = _videoInfo.Title;
            }
        }

        private bool GetYoutubeInfo()
        {
            _videoInfo = null;

            if (string.IsNullOrEmpty(YoutubeLink))
            {
                _uiServices.ShowMessageBox("You need to enter a valid Youtube URL", "Missing URL", Buttons.OK, Icon.Warning);
            }

            try
            {
                _videoInfo = _youtubeManager.GetInfo(YoutubeLink);
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
    }
}