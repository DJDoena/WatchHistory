namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using AbstractionLayer.IOServices;
    using DoenaSoft.AbstractionLayer.IOServices.Implementations;
    using MediaInfoHelper.Youtube;
    using WatchHistory.Implementations;

    internal sealed class ManualWatchesProcessor
    {
        private readonly IIOServices _ioServices;

        public ManualWatchesProcessor(IIOServices ioServices)
        {
            _ioServices = ioServices;
        }

        internal void Update(FileEntry entry)
        {
            var info = TryGetInfo(entry);

            if (info == null)
            {
                return;
            }

            entry.Title = info.Title;

            if (info.RunningTime > 0)
            {
                entry.VideoLength = info.RunningTime;
            }

            entry.CreationTime = (new FileInfo(entry.FullName)).CreationTimeUtc;
        }

        internal ManualVideoInfo TryGetInfo(FileEntry entry)
        {
            ManualVideoInfo info = null;
            if (_ioServices.File.Exists(entry.FullName))
            {
                try
                {
                    info = SerializerHelper.Deserialize<ManualVideoInfo>(_ioServices, entry.FullName);
                }
                catch
                { }
            }

            return info;
        }

        internal static ManualVideoInfo CreateInfo(string title, uint length, string note) => new ManualVideoInfo()
        {
            Title = title,
            RunningTime = length,
            Note = note,
        };
    }
}