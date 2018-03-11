namespace DoenaSoft.WatchHistory.Main.Implementations
{
    using System;
    using System.Xml.Serialization;
    using DVDProfiler.DVDProfilerXML.Version400;

    [XmlRoot]
    public sealed class DvdWatches
    {
        public String Title;

        public Event[] Watches;
    }
}