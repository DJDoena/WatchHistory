namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System.Text;
    using AbstractionLayer.IOServices;
    using DoenaSoft.MediaInfoHelper.DataObjects.VideoMetaXml;
    using ToolBox.Extensions;
    using WatchHistory.Implementations;

    internal sealed class VideoInfoAdder
    {
        private readonly IIOServices _ioServices;

        private readonly FileEntry _entry;

        public VideoInfoAdder(IIOServices ioServices, FileEntry entry)
        {
            _ioServices = ioServices;
            _entry = entry;
        }

        internal void Add()
        {
            var xmlFile = _entry.FullName + ".xml";

            if (_ioServices.File.Exists(xmlFile))
            {
                try
                {
                    this.TryAddVideoInfo(xmlFile);
                }
                catch
                { }
            }
        }

        private void TryAddVideoInfo(string xmlFile)
        {
            var info = SerializerHelper.Deserialize<VideoInfoDocument>(_ioServices, xmlFile);

            _entry.VideoLength = info.VideoInfo.Duration;

            _entry.Title = BuildTitle(info.VideoInfo.Episode);
        }

        private static string BuildTitle(Episode episode)
        {
            var title = new StringBuilder();

            if (episode != null)
            {
                AddTitlePart(title, episode.SeriesName);
                AddTitlePart(title, episode.EpisodeNumber);
                AddTitlePart(title, episode.EpisodeName);
            }

            return (title.ToString().TrimEnd());
        }

        private static void AddTitlePart(StringBuilder title, string part)
        {
            if (part.IsNotEmpty())
            {
                title.Append(part);
                title.Append(" ");
            }
        }
    }
}