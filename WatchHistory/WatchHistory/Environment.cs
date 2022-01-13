namespace DoenaSoft.WatchHistory
{
    using AbstractionLayer.IOServices;

    internal static class Environment
    {
        internal static string SettingsFile { get; private set; }

        internal static string DataFile { get; private set; }

        internal static string MyDocumentsFolder { get; private set; }

        internal static void Init(IIOServices ioServices)
        {
            MyDocumentsFolder = GetMyDocumentsFolder(ioServices);

            SettingsFile = ioServices.Path.Combine(MyDocumentsFolder, "Settings.xml");

            DataFile = ioServices.Path.Combine(MyDocumentsFolder, "Files.xml");
        }

        private static string GetMyDocumentsFolder(IIOServices ioServices)
        {
            var myDocumentsFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);

            myDocumentsFolder = ioServices.Path.Combine(myDocumentsFolder, "WatchHistory");

            if (ioServices.Folder.Exists(myDocumentsFolder) == false)
            {
                ioServices.Folder.CreateFolder(myDocumentsFolder);
            }

            return myDocumentsFolder;
        }
    }
}