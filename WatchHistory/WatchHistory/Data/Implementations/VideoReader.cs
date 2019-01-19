namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System;
    using AbstractionLayer.IOServices;
    using ToolBox.Generics;
    using WatchHistory.Data;
    using WatchHistory.Implementations;

    internal sealed class VideoReader
    {
        private readonly IIOServices _IOServices;

        private readonly FileEntry _fileEntry;

        private String FullName => _fileEntry.FullName;

        public VideoReader(IIOServices ioServices, FileEntry entry)
        {
            _IOServices = ioServices;
            _fileEntry = entry;
        }

        internal UInt32 GetLength()
        {
            UInt32 seconds = 0;

            if ((FullName.EndsWith(Constants.DvdProfilerFileExtension) == false) && (_IOServices.File.Exists(FullName)))
            {
                seconds = GetLengthFromFile();
            }

            return seconds;
        }

        private UInt32 GetLengthFromFile()
        {
            UInt32 videoLength = GetLengthFromMeta();

            if (videoLength > 0)
            {
                return videoLength;
            }

            videoLength = GetDuration();

            return videoLength;
        }

        private UInt32 GetLengthFromMeta()
        {
            String xmlFile = FullName + ".xml";

            if (_IOServices.File.Exists(xmlFile))
            {
                try
                {
                    VideoInfo info = SerializerHelper.Deserialize<VideoInfo>(_IOServices, xmlFile);

                    return info.Duration;
                }
                catch
                { }
            }

            return 0;
        }

        private UInt32 GetDuration()
        {
            FFProbe mediaInfo = GetMediaInfo();

            if (mediaInfo != null)
            {
                var xmlInfo = MediaInfo2XmlConverter.Convert(mediaInfo);

                return xmlInfo.DurationSpecified ? xmlInfo.Duration : 0;
            }

            return 0;
        }

        private FFProbe GetMediaInfo()
        {
            try
            {
                var mediaInfo = (new NReco.VideoInfo.FFProbe()).GetMediaInfo(FullName);

                var xml = mediaInfo.Result.CreateNavigator().OuterXml;

                var ffprobe = Serializer<FFProbe>.FromString(xml);

                return (ffprobe);
            }
            catch
            {
                return (null);
            }
        }
    }
}