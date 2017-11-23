namespace DoenaSoft.WatchHistory
{
    using System;
    using AbstractionLayer.IOServices;

    internal static class Environment
    {
        internal static String SettingsFile { get; private set; }

        internal static String DataFile { get; private set; }

        internal static void Init(IIOServices ioServices)
        {
            String appDataFolder = GetAppDataFolder(ioServices);

            SettingsFile = ioServices.Path.Combine(appDataFolder, "Settings.xml");

            DataFile = ioServices.Path.Combine(appDataFolder, "Files.xml");
        }

        private static String GetAppDataFolder(IIOServices ioServices)
        {
            String appDataFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);

            appDataFolder = ioServices.Path.Combine(appDataFolder, "Doena Soft.", "WatchHistory");

            if (ioServices.Directory.Exists(appDataFolder) == false)
            {
                ioServices.Directory.CreateFolder(appDataFolder);
            }

            return (appDataFolder);
        }
    }
}
