using System.Xml.Serialization;

namespace DoenaSoft.WatchHistory.Data;

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
        this.Users = [Constants.DefaultUser];

        this.FileExtensions = ["avi", "mp4", "mkv"];

        this.HideDeleted = true;
    }

    public bool HideDeleted { get; set; }
}