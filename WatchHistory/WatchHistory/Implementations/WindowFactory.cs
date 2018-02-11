namespace DoenaSoft.WatchHistory.Implementations
{
    using System;
    using System.Linq;
    using System.Windows;
    using AbstractionLayer.IOServices;
    using AbstractionLayer.UIServices;
    using Data;
    using Ignore;
    using Ignore.Implementations;
    using Main;
    using Main.Implementations;
    using SelectUser;
    using SelectUser.Implementations;
    using Settings;
    using Settings.Implementations;

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

                Window window = new SelectUserWindow()
                {
                    DataContext = viewModel
                };

                window.Show();
            }
            else
            {
                String user = DataManager.Users.FirstOrDefault() ?? Constants.DefaultUser;

                OpenMainWindow(user);
            }
        }

        public void OpenMainWindow(String userName)
        {
            IMainModel model = new MainModel(DataManager, IOServices, UIServices, userName);

            IMainViewModel viewModel = new MainViewModel(model, DataManager, IOServices, this, userName);

            Window window = new MainWindow()
            {
                DataContext = viewModel
            };

            window.Show();

            DataManager.Resume();
        }

        public void OpenSettingsWindow()
        {
            ISettingsViewModel viewModel = new SettingsViewModel(DataManager, UIServices);

            Window window = new SettingsWindow()
            {
                DataContext = viewModel
            };

            window.ShowDialog();
        }

        public void OpenIgnoreWindow(String userName)
        {
            IIgnoreModel model = new IgnoreModel(DataManager, userName);

            IIgnoreViewModel viewModel = new IgnoreViewModel(model, DataManager, IOServices, this, userName);

            Window window = new IgnoreWindow()
            {
                DataContext = viewModel
            };

            window.ShowDialog();
        }

        #endregion
    }
}