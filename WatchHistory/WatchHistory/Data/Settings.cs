using System.Xml.Serialization;

namespace DoenaSoft.WatchHistory.Data
{
    [XmlRoot]
    public sealed class Settings
    {
        public DefaultValues DefaultValues { get; set; }
    }
}