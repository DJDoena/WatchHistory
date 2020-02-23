namespace DoenaSoft.WatchHistory.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AbstractionLayer.IOServices;
    using AbstractionLayer.UIServices;
    using Data;
    using Main.Implementations;

    internal sealed class WindowFactory : IWindowFactory
    {
        private readonly IIOServices _ioServices;

        private readonly IUIServices _uiServices;

        private readonly IClipboardServices _clipboardServices;

        private readonly IDataManager _dataManager;

        private readonly AddYoutubeLink.IYoutubeManager _youtubeManager;

        public WindowFactory(IIOServices ioServices
            , IUIServices uiServices
            , IClipboardServices clipboardServices
            , IDataManager dataManager
            , AddYoutubeLink.IYoutubeManager youtubeManager)
        {
            _ioServices = ioServices;
            _uiServices = uiServices;
            _clipboardServices = clipboardServices;
            _dataManager = dataManager;
            _youtubeManager = youtubeManager;
        }

        #region IWindowFactory

        public void OpenSelectUserWindow()
        {
            if (_dataManager.Users.Count() > 1)
            {
                var viewModel = new SelectUser.Implementations.SelectUserViewModel(_dataManager, this);

                var window = new SelectUser.Implementations.SelectUserWindow()
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
            var viewModel = new WatchHistory.Settings.Implementations.SettingsViewModel(_dataManager, _uiServices);

            var window = new WatchHistory.Settings.Implementations.SettingsWindow()
            {
                DataContext = viewModel,
            };

            window.ShowDialog();
        }

        public void OpenIgnoreWindow(string userName, string filter)
        {
            var model = new IgnoreEntry.Implementations.IgnoreEntryModel(_dataManager, userName);

            var viewModel = new IgnoreEntry.Implementations.IgnoreEntryViewModel(model, _dataManager, _ioServices, userName)
            {
                Filter = filter,
            };

            var window = new IgnoreEntry.Implementations.IgnoreEntryWindow()
            {
                DataContext = viewModel,
            };

            window.ShowDialog();
        }

        public DateTime? OpenWatchedOnWindow()
        {
            var viewModel = new AddWatchedOn.Implementations.AddWatchedOnViewModel();

            var window = new AddWatchedOn.Implementations.AddWatchedOnWindow()
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
            var viewModel = new EditRunningTime.Implementations.EditRunningTimeViewModel(seconds);

            var window = new EditRunningTime.Implementations.EditRunningTimeWindow()
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
            var viewModel = new ShowWatches.Implementations.ShowWatchesViewModel(watches);

            var window = new ShowWatches.Implementations.ShowWatchesWindow()
            {
                DataContext = viewModel,
            };

            window.ShowDialog();
        }

        public void OpenAddYoutubeLinkWindow(string userName)
        {
            var viewModel = new AddYoutubeLink.Implementations.AddYoutubeLinkViewModel(_dataManager, _ioServices, _uiServices, _clipboardServices, _youtubeManager, userName);

            var window = new AddYoutubeLink.Implementations.AddYoutubeLinkWindow()
            {
                DataContext = viewModel,
            };

            window.ShowDialog();
        }

        public void OpenAddManualEntryWindow(string userName)
        {
            var viewModel = new AddManualEntry.Implementations.AddManualEntryViewModel(_dataManager, _ioServices, _uiServices, userName);

            var window = new AddManualEntry.Implementations.AddManualEntryWindow()
            {
                DataContext = viewModel,
            };

            window.ShowDialog();
        }

        public string OpenEditTitleWindow(string title)
        {
            var viewModel = new EditTitle.Implementations.EditTitleViewModel(title);

            var window = new EditTitle.Implementations.EditTitleWindow()
            {
                DataContext = viewModel,
            };

            if (window.ShowDialog() == true)
            {
                return viewModel.Title;
            }

            return null;
        }

        #endregion
    }
}