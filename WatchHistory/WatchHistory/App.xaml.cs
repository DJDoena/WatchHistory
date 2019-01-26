namespace DoenaSoft.WatchHistory
{
    using System;
    using System.Windows;
    using AbstractionLayer.IOServices;
    using AbstractionLayer.IOServices.Implementations;
    using AbstractionLayer.UIServices;
    using AbstractionLayer.UIServices.Implementations;
    using Data;
    using Data.Implementations;
    using DVDProfiler.DVDProfilerHelper;
    using Implementations;
    using WatchHistory.YoutubeLink.Implementations;

    public partial class App : Application
    {
        private IUIServices UIServices { get; set; }

        private IIOServices IOServices { get; set; }

        private IDataManager DataManager { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            IOServices = new IOServices();

            UIServices = new WindowUIServices();

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            Environment.Init(IOServices);

            DataManager = new DataManager(Environment.SettingsFile, Environment.DataFile, IOServices);

            var youtubeManager = new YoutubeManager();

            IWindowFactory windowFactory = new WindowFactory(IOServices, UIServices, DataManager, youtubeManager);

            windowFactory.OpenSelectUserWindow();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            DataManager.SaveSettingsFile();
            DataManager.SaveDataFile();
        }

        private void OnUnhandledException(Object sender
            , UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;

            if (ex != null)
            {
                ExceptionXml exceptionXml = new ExceptionXml(ex);

                String fileName = IOServices.Path.Combine(Environment.AppDataFolder, "Crash.xml");

                SerializerHelper.Serialize(IOServices, fileName, exceptionXml);

                UIServices.ShowMessageBox(ex.Message, String.Empty, Buttons.OK, Icon.Error);
            }
            else
            {
                UIServices.ShowMessageBox(e.ExceptionObject?.ToString() ?? "Error", String.Empty, Buttons.OK, Icon.Error);
            }
        }
    }
}