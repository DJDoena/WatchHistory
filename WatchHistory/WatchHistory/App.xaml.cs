namespace DoenaSoft.WatchHistory
{
    using System;
    using System.Diagnostics;
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
        private bool _cancelStartUp;

        private IUIServices UIServices { get; set; }

        private IIOServices IOServices { get; set; }

        private IDataManager DataManager { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            _cancelStartUp = false;

            this.UIServices = new WindowUIServices();

            var processes = Process.GetProcessesByName("WatchHistory");

            if (processes.Length > 1)
            {
                if (this.UIServices.ShowMessageBox("There's already an instance running. If you start another one, you could invalidate your cache. Continue?"
                    , "Continue?", Buttons.YesNo, Icon.Error) == Result.No)
                {
                    _cancelStartUp = true;

                    this.Shutdown(0);

                    return;
                }
            }

            this.IOServices = new IOServices();

            var clipboardServices = new WindowClipboardServices();

            AppDomain.CurrentDomain.UnhandledException += this.OnUnhandledException;

            (new DataUpgrader(this.IOServices)).Upgrade();

            Environment.Init(this.IOServices);

            this.DataManager = new DataManager(Environment.SettingsFile, Environment.DataFile, this.IOServices);

            var youtubeManager = new AddYoutubeLink.Implementations.YoutubeManager();

            var windowFactory = new WindowFactory(this.IOServices, this.UIServices, clipboardServices, this.DataManager, youtubeManager);

            windowFactory.OpenSelectUserWindow();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (!_cancelStartUp)
            {
                this.DataManager.SaveSettingsFile();
                this.DataManager.SaveDataFile();
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                var exceptionXml = new ExceptionXml(ex);

                var fileName = this.IOServices.Path.Combine(Environment.MyDocumentsFolder, "Crash.xml");

                SerializerHelper.Serialize(this.IOServices, fileName, exceptionXml);

                this.UIServices.ShowMessageBox(ex.Message, string.Empty, Buttons.OK, Icon.Error);
            }
            else
            {
                this.UIServices.ShowMessageBox(e.ExceptionObject?.ToString() ?? "Error", string.Empty, Buttons.OK, Icon.Error);
            }
        }
    }
}