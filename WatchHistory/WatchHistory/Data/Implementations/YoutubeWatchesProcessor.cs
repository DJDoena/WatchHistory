﻿namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using AbstractionLayer.IOServices;
    using WatchHistory.Implementations;

    internal sealed class YoutubeWatchesProcessor
    {
        private readonly IIOServices _ioServices;

        public YoutubeWatchesProcessor(IIOServices ioServices)
        {
            _ioServices = ioServices;
        }

        internal void Update(FileEntry entry)
        {
            YoutubeVideoInfo info = null;

            if (_ioServices.File.Exists(entry.FullName))
            {
                try
                {
                    info = SerializerHelper.Deserialize<YoutubeVideoInfo>(_ioServices, entry.FullName);
                }
                catch
                { }
            }

            if (info == null)
            {
                return;
            }

            entry.Title = info.Title;

            if (info.RunningTime > 0)
            {
                entry.VideoLength = info.RunningTime;
            }

            entry.CreationTime = info.Published.Conform();
        }
    }
}