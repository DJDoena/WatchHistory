using DoenaSoft.AbstractionLayer.IOServices;
using DoenaSoft.MediaInfoHelper.DataObjects;
using DoenaSoft.WatchHistory.Implementations;

namespace DoenaSoft.WatchHistory.Data.Implementations
{
    internal sealed class ManualWatchesProcessor
    {
        private readonly IIOServices _ioServices;

        public ManualWatchesProcessor(IIOServices ioServices)
        {
            _ioServices = ioServices;
        }

        internal void Update(FileEntry entry)
        {
            var info = this.TryGetInfo(entry);

            if (info == null)
            {
                return;
            }

            entry.Title = info.Title;

            if (info.RunningTime > 0)
            {
                entry.VideoLength = info.RunningTime;
            }

            entry.CreationTime = (_ioServices.GetFile(entry.FullName)).CreationTimeUtc;
        }

        internal ManualVideo TryGetInfo(FileEntry entry)
        {
            ManualVideo info = null;
            if (_ioServices.File.Exists(entry.FullName))
            {
                try
                {
                    info = SerializerHelper.Deserialize<ManualVideo>(_ioServices, entry.FullName);
                }
                catch
                { }
            }

            return info;
        }

        internal static ManualVideo CreateInfo(string title, uint length, string note) => new ManualVideo()
        {
            Title = title,
            RunningTime = length,
            Note = note,
        };
    }
}