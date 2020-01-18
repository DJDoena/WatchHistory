namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System.Text;
    using AbstractionLayer.IOServices;
    using MediaInfoHelper;
    using ToolBox.Extensions;
    using WatchHistory.Implementations;

    internal sealed class VideoInfoAdder
    {
        private readonly IIOServices _ioServices;

        private readonly FileEntry _fileEntry;

        public VideoInfoAdder(IIOServices ioServices
            , FileEntry fileEntry)
        {
            _ioServices = ioServices;
            _fileEntry = fileEntry;
        }

        internal void Add()
        {
            var xmlFile = _fileEntry.FullName + ".xml";

            if (_ioServices.File.Exists(xmlFile))
            {
                try
                {
                    TryAddVideoInfo(xmlFile);
                }
                catch
                { }
            }
        }

        private void TryAddVideoInfo(string xmlFile)
        {
            var info = SerializerHelper.Deserialize<Doc>(_ioServices, xmlFile);

            _fileEntry.VideoLength = info.VideoInfo.Duration;

            _fileEntry.Title = BuildTitle(info.VideoInfo.Episode);
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

        private static void AddTitlePart(StringBuilder title
            , string part)
        {
            if (part.IsNotEmpty())
            {
                title.Append(part);
                title.Append(" ");
            }
        }
    }
}