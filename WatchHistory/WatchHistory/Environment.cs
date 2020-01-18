namespace DoenaSoft.WatchHistory
{
    using AbstractionLayer.IOServices;

    internal static class Environment
    {
        internal static string SettingsFile { get; private set; }

        internal static string DataFile { get; private set; }

        internal static string AppDataFolder { get; private set; }

        internal static void Init(IIOServices ioServices)
        {
            AppDataFolder = GetAppDataFolder(ioServices);

            SettingsFile = ioServices.Path.Combine(AppDataFolder, "Settings.xml");

            DataFile = ioServices.Path.Combine(AppDataFolder, "Files.xml");
        }

        private static string GetAppDataFolder(IIOServices ioServices)
        {
            string appDataFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);

            appDataFolder = ioServices.Path.Combine(appDataFolder, "Doena Soft.", "WatchHistory");

            if (ioServices.Folder.Exists(appDataFolder) == false)
            {
                ioServices.Folder.CreateFolder(appDataFolder);
            }

            return (appDataFolder);
        }
    }
}