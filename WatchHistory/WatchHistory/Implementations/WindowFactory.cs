using System;
using System.Linq;
using System.Windows;
using DoenaSoft.AbstractionLayer.IOServices;
using DoenaSoft.AbstractionLayer.UIServices;
using DoenaSoft.WatchHistory.Data;
using DoenaSoft.WatchHistory.Ignore;
using DoenaSoft.WatchHistory.Ignore.Implementations;
using DoenaSoft.WatchHistory.Main;
using DoenaSoft.WatchHistory.Main.Implementations;
using DoenaSoft.WatchHistory.SelectUser;
using DoenaSoft.WatchHistory.SelectUser.Implementations;
using DoenaSoft.WatchHistory.Settings;
using DoenaSoft.WatchHistory.Settings.Implementations;

namespace DoenaSoft.WatchHistory.Implementations
{
    internal sealed class WindowFactory : IWindowFactory
    {
        private readonly IIOServices IOServices;

        private readonly IUIServices UIServices;

        private readonly IDataManager DataManager;

        private Window WaitWindow { get; set; }

        public WindowFactory(IIOServices ioServices
            , IUIServices uiServices
            , IDataManager dataManager)
        {
            IOServices = ioServices;
            UIServices = uiServices;
            DataManager = dataManager;
        }

        #region IWindowFactory

        public void OpenSelectUserWindow()
        {
            if (DataManager.Users.Count() > 1)
            {
                ISelectUserViewModel viewModel = new SelectUserViewModel(DataManager, this);

                Window window = new SelectUserWindow();

                window.DataContext = viewModel;

                window.Show();
            }
            else
            {
                String user = DataManager.Users.FirstOrDefault() ?? "default";

                OpenMainWindow(user);
            }
        }

        public void OpenMainWindow(String userName)
        {
            IMainModel model = new MainModel(DataManager, IOServices, UIServices, userName);

            IMainViewModel viewModel = new MainViewModel(model, DataManager, IOServices, this, userName);

            Window window = new MainWindow();

            window.DataContext = viewModel;

            window.Show();
        }

        public void OpenSettingsWindow()
        {
            ISettingsViewModel viewModel = new SettingsViewModel(DataManager, UIServices);

            Window window = new SettingsWindow();

            window.DataContext = viewModel;

            window.ShowDialog();
        }

        public void OpenIgnoreWindow(String userName)
        {
            IIgnoreModel model = new IgnoreModel(DataManager, userName);

            IIgnoreViewModel viewModel = new IgnoreViewModel(model, DataManager, IOServices, this, userName);

            Window window = new IgnoreWindow();

            window.DataContext = viewModel;

            window.ShowDialog();
        }

        #endregion
    }
}