namespace DoenaSoft.WatchHistory.Data
{
    using System.Xml.Serialization;

    public sealed class DefaultValues
    {
        [XmlArray("Users")]
        [XmlArrayItem("User")]
        public string[] Users { get; set; }

        [XmlArray("RootFolders")]
        [XmlArrayItem("RootFolder")]
        public string[] RootFolders { get; set; }

        [XmlArray("FileExtensions")]
        [XmlArrayItem("FileExtension")]
        public string[] FileExtensions { get; set; }

        public DefaultValues()
        {
            Users = new[] { Constants.DefaultUser };

            FileExtensions = new[] { "avi", "mp4", "mkv" };
        }
    }
}