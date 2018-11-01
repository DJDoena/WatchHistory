namespace DoenaSoft.WatchHistory.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using AbstractionLayer.IOServices;
    using AbstractionLayer.UIServices;
    using Data;
    using DoenaSoft.WatchHistory.RunningTime;
    using DoenaSoft.WatchHistory.RunningTime.Implementations;
    using Ignore;
    using Ignore.Implementations;
    using Main;
    using Main.Implementations;
    using SelectUser;
    using SelectUser.Implementations;
    using Settings;
    using Settings.Implementations;
    using WatchedOn;
    using WatchedOn.Implementations;
    using Watches;
    using Watches.Implementations;

    internal sealed class WindowFactory : IWindowFactory
    {
        private readonly IIOServices _IOServices;

        private readonly IUIServices _UIServices;

        private readonly IDataManager _DataManager;

        private Window WaitWindow { get; set; }

        public WindowFactory(IIOServices ioServices
            , IUIServices uiServices
            , IDataManager dataManager)
        {
            _IOServices = ioServices;
            _UIServices = uiServices;
            _DataManager = dataManager;
        }

        #region IWindowFactory

        public void OpenSelectUserWindow()
        {
            if (_DataManager.Users.Count() > 1)
            {
                ISelectUserViewModel viewModel = new SelectUserViewModel(_DataManager, this);

                Window window = new SelectUserWindow()
                {
                    DataContext = viewModel
                };

                window.Show();
            }
            else
            {
                String user = _DataManager.Users.FirstOrDefault() ?? Constants.DefaultUser;

                OpenMainWindow(user);
            }
        }

        public void OpenMainWindow(String userName)
        {
            IMainModel model = new MainModel(_DataManager, _IOServices, _UIServices, userName);

            IMainViewModel viewModel = new MainViewModel(model, _DataManager, _IOServices, this, userName);

            Window window = new MainWindow()
            {
                DataContext = viewModel
            };

            window.Show();

            _DataManager.Resume();
        }

        public void OpenSettingsWindow()
        {
            ISettingsViewModel viewModel = new SettingsViewModel(_DataManager, _UIServices);

            Window window = new SettingsWindow()
            {
                DataContext = viewModel
            };

            window.ShowDialog();
        }

        public void OpenIgnoreWindow(String userName)
        {
            IIgnoreModel model = new IgnoreModel(_DataManager, userName);

            IIgnoreViewModel viewModel = new IgnoreViewModel(model, _DataManager, _IOServices, this, userName);

            Window window = new IgnoreWindow()
            {
                DataContext = viewModel
            };

            window.ShowDialog();
        }

        public Nullable<DateTime> OpenWatchedOnWindow()
        {
            IWatchedOnViewModel viewModel = new WatchedOnViewModel();

            Window window = new WatchedOnWindow()
            {
                DataContext = viewModel
            };

            if (window.ShowDialog() == true)
            {
                return (viewModel.WatchedOn);
            }

            return (null);
        }

        public Nullable<UInt32> OpenRunningTimeWindow(UInt32 seconds)
        {
            IRunningTimeViewModel viewModel = new RunningTimeViewModel(seconds);

            Window window = new RunningTimeWindow()
            {
                DataContext = viewModel
            };

            if (window.ShowDialog() == true)
            {
                return (viewModel.RunningTime);
            }

            return (null);
        }

        public void OpenWatchesWindow(IEnumerable<Watch> watches)
        {
            IWatchesViewModel viewModel = new WatchesViewModel(watches);

            Window window = new WatchesWindow()
            {
                DataContext = viewModel
            };

            window.ShowDialog();
        }

        #endregion
    }
}