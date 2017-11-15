namespace DoenaSoft.WatchHistory.Data
{
    using System;
    using System.Xml.Serialization;

    public sealed class DefaultValues
    {
        [XmlArray("Users")]
        [XmlArrayItem("User")]
        public String[] Users { get; set; }

        [XmlArray("RootFolders")]
        [XmlArrayItem("RootFolder")]
        public String[] RootFolders { get; set; }

        [XmlArray("FileExtensions")]
        [XmlArrayItem("FileExtension")]
        public String[] FileExtensions { get; set; }

        public DefaultValues()
        {
            Users = new[] { Constants.DefaultUser };

            FileExtensions = new[] { "avi", "mp4", "mkv" };
        }
    }
}