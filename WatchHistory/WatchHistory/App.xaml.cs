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

    public partial class App : Application
    {
        private IUIServices UIServices { get; set; }

        private IIOServices IOServices { get; set; }

        private IDataManager DataManager { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            IOServices = new IOServices();

            UIServices = new WindowUIServices();

            var clipboardServices = new WindowClipboardServices();

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            Environment.Init(IOServices);

            DataManager = new DataManager(Environment.SettingsFile, Environment.DataFile, IOServices);

            var youtubeManager = new AddYoutubeLink.Implementations.YoutubeManager();

            IWindowFactory windowFactory = new WindowFactory(IOServices, UIServices, clipboardServices, DataManager, youtubeManager);

            windowFactory.OpenSelectUserWindow();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            DataManager.SaveSettingsFile();
            DataManager.SaveDataFile();
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                var exceptionXml = new ExceptionXml(ex);

                var fileName = IOServices.Path.Combine(Environment.AppDataFolder, "Crash.xml");

                SerializerHelper.Serialize(IOServices, fileName, exceptionXml);

                UIServices.ShowMessageBox(ex.Message, string.Empty, Buttons.OK, Icon.Error);
            }
            else
            {
                UIServices.ShowMessageBox(e.ExceptionObject?.ToString() ?? "Error", string.Empty, Buttons.OK, Icon.Error);
            }
        }
    }
}