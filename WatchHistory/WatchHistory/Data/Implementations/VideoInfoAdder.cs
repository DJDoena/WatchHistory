namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System;
    using System.IO;
    using System.Text;
    using AbstractionLayer.IOServices;
    using ToolBox.Extensions;
    using ToolBox.Generics;

    internal sealed class VideoInfoAdder
    {
        private readonly IIOServices _IOServices;

        private readonly FileEntry _FileEntry;

        public VideoInfoAdder(IIOServices ioServices
            , FileEntry fileEntry)
        {
            _IOServices = ioServices;
            _FileEntry = fileEntry;
        }

        internal void Add()
        {
            String xmlFile = _FileEntry.FullName + ".xml";

            if (_IOServices.File.Exists(xmlFile))
            {
                try
                {
                    TryAddVideoInfo(xmlFile);
                }
                catch
                { }
            }
        }

        private void TryAddVideoInfo(String xmlFile)
        {
            VideoInfo info = GetVideoInfo(xmlFile);

            _FileEntry.VideoLength = info.Duration;

            _FileEntry.Title = BuildTitle(info.Episode);
        }

        private VideoInfo GetVideoInfo(String xmlFile)
        {
            using (Stream fs = _IOServices.GetFileStream(xmlFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                VideoInfo info = Serializer<VideoInfo>.Deserialize(fs);

                return (info);
            }
        }

        private static String BuildTitle(Episode episode)
        {
            StringBuilder title = new StringBuilder();

            if (episode != null)
            {
                AddTitlePart(title, episode.SeriesName);
                AddTitlePart(title, episode.EpisodeNumber);
                AddTitlePart(title, episode.EpisodeName);
            }

            return (title.ToString().TrimEnd());
        }

        private static void AddTitlePart(StringBuilder title
            , String part)
        {
            if (part.IsNotEmpty())
            {
                title.Append(part);
                title.Append(" ");
            }
        }
    }
}