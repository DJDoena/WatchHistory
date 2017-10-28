using System;
using System.Xml.Serialization;

namespace DoenaSoft.WatchHistory.Data
{
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
            Users = new[] { "default" };

            FileExtensions = new[] { "avi", "mp4", "mkv" };
        }
    }
}