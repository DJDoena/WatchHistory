using System;

namespace DoenaSoft.WatchHistory.Data
{
    public sealed class YoutubeVideoInfo
    {
        public string Id { get; set; }

        public uint RunningTime { get; set; }

        public string Title { get; set; }

        public DateTime Published { get; set; }
    }
}
