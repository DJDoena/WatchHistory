namespace DoenaSoft.WatchHistory.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using AbstractionLayer.IOServices;
    using AbstractionLayer.UIServices;
    using Data;
    using Ignore.Implementations;
    using Main.Implementations;
    using SelectUser.Implementations;
    using Settings.Implementations;
    using WatchedOn.Implementations;
    using Watches.Implementations;
    using WatchHistory.RunningTime.Implementations;
    using WatchHistory.YoutubeLink;
    using WatchHistory.YoutubeLink.Implementations;

    internal sealed class WindowFactory : IWindowFactory
    {
        private readonly IIOServices _ioServices;

        private readonly IUIServices _uiServices;

        private readonly IDataManager _dataManager;

        private readonly IYoutubeManager _youtubeManager;

        private Window WaitWindow { get; set; }

        public WindowFactory(IIOServices ioServices
            , IUIServices uiServices
            , IDataManager dataManager
            , IYoutubeManager youtubeManager)
        {
            _ioServices = ioServices;
            _uiServices = uiServices;
            _dataManager = dataManager;
            _youtubeManager = youtubeManager;
        }

        #region IWindowFactory

        public void OpenSelectUserWindow()
        {
            if (_dataManager.Users.Count() > 1)
            {
                var viewModel = new SelectUserViewModel(_dataManager, this);

                var window = new SelectUserWindow()
                {
                    DataContext = viewModel,
                };

                window.Show();
            }
            else
            {
                var user = _dataManager.Users.FirstOrDefault() ?? Constants.DefaultUser;

                OpenMainWindow(user);
            }
        }

        public void OpenMainWindow(string userName)
        {
            var model = new MainModel(_dataManager, _ioServices, _uiServices, userName);

            var viewModel = new MainViewModel(model, _dataManager, _ioServices, this, userName);

            var window = new MainWindow()
            {
                DataContext = viewModel,
            };

            window.Show();

            _dataManager.Resume();
        }

        public void OpenSettingsWindow()
        {
            var viewModel = new SettingsViewModel(_dataManager, _uiServices);

            var window = new SettingsWindow()
            {
                DataContext = viewModel,
            };

            window.ShowDialog();
        }

        public void OpenIgnoreWindow(string userName, string filter)
        {
            var model = new IgnoreModel(_dataManager, userName);

            var viewModel = new IgnoreViewModel(model, _dataManager, _ioServices, this, userName)
            {
                Filter = filter,
            };

            var window = new IgnoreWindow()
            {
                DataContext = viewModel,
            };

            window.ShowDialog();
        }

        public Nullable<DateTime> OpenWatchedOnWindow()
        {
            var viewModel = new WatchedOnViewModel();

            var window = new WatchedOnWindow()
            {
                DataContext = viewModel,
            };

            if (window.ShowDialog() == true)
            {
                return (viewModel.WatchedOn);
            }

            return null;
        }

        public uint? OpenRunningTimeWindow(uint seconds)
        {
            var viewModel = new RunningTimeViewModel(seconds);

            var window = new RunningTimeWindow()
            {
                DataContext = viewModel,
            };

            if (window.ShowDialog() == true)
            {
                return viewModel.RunningTime;
            }

            return null;
        }

        public void OpenWatchesWindow(IEnumerable<Watch> watches)
        {
            var viewModel = new WatchesViewModel(watches);

            var window = new WatchesWindow()
            {
                DataContext = viewModel,
            };

            window.ShowDialog();
        }

        public void OpenAddYoutubeLinkVideo(string userName)
        {
            var viewModel = new YoutubeLinkViewModel(_dataManager, _ioServices, _uiServices, _youtubeManager, userName);

            var window = new YoutubeLinkWindow()
            {
                DataContext = viewModel,
            };

            window.ShowDialog();
        }

        #endregion
    }
}