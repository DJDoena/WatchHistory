namespace DoenaSoft.WatchHistory.Data
{
    using System.Xml.Serialization;

    [XmlRoot]
    public sealed class Settings
    {
        public DefaultValues DefaultValues { get; set; }
    }
}