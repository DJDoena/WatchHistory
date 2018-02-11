namespace DoenaSoft.WatchHistory.Main.Implementations
{
    using System.Xml.Serialization;
    using DVDProfiler.DVDProfilerXML.Version390;

    [XmlRoot]
    public sealed class DvdWatches
    {
        public Event[] Watches;
    }
}