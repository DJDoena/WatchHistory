namespace DoenaSoft.WatchHistory
{
    using System;
    using AbstractionLayer.IOServices;

    internal static class Environment
    {
        internal static String SettingsFile { get; private set; }

        internal static String DataFile { get; private set; }

        internal static String AppDataFolder { get; private set; }

        internal static void Init(IIOServices ioServices)
        {
            AppDataFolder = GetAppDataFolder(ioServices);

            SettingsFile = ioServices.Path.Combine(AppDataFolder, "Settings.xml");

            DataFile = ioServices.Path.Combine(AppDataFolder, "Files.xml");
        }

        private static String GetAppDataFolder(IIOServices ioServices)
        {
            String appDataFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);

            appDataFolder = ioServices.Path.Combine(appDataFolder, "Doena Soft.", "WatchHistory");

            if (ioServices.Folder.Exists(appDataFolder) == false)
            {
                ioServices.Folder.CreateFolder(appDataFolder);
            }

            return (appDataFolder);
        }
    }
}