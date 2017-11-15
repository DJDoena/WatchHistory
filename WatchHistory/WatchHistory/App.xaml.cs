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

        private String SettingsFile { get; set; }

        internal static String DataFile { get; private set; }

        private IDataManager DataManager { get; set; }

        internal static String AppDataFolder { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            IOServices = new IOServices();

            UIServices = new WindowUIServices();

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            AppDataFolder = GetAppDataFolder();

            SettingsFile = IOServices.Path.Combine(AppDataFolder, "Settings.xml");

            DataFile = IOServices.Path.Combine(AppDataFolder, "Files.xml");

            DataManager = new DataManager(SettingsFile, DataFile, IOServices);

            IWindowFactory windowFactory = new WindowFactory(IOServices, UIServices, DataManager);

            windowFactory.OpenSelectUserWindow();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            DataManager.SaveSettingsFile(SettingsFile);
            DataManager.SaveDataFile(DataFile);
        }

        private String GetAppDataFolder()
        {
            String appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            appData = IOServices.Path.Combine(appData, "Doena Soft.", "WatchHistory");

            if (IOServices.Directory.Exists(appData) == false)
            {
                IOServices.Directory.CreateFolder(appData);
            }

            return (appData);
        }

        private void OnUnhandledException(Object sender
            , UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;

            if (ex != null)
            {
                ExceptionXml exceptionXml = new ExceptionXml(ex);

                String fileName = IOServices.Path.Combine(AppDataFolder, "Crash.xml");

                Serializer<ExceptionXml>.Serialize(fileName, exceptionXml);

                UIServices.ShowMessageBox(ex.Message, String.Empty, Buttons.OK, Icon.Error);
            }
            else
            {
                UIServices.ShowMessageBox(e.ExceptionObject?.ToString() ?? "Error", String.Empty, Buttons.OK, Icon.Error);
            }
        }
    }
}