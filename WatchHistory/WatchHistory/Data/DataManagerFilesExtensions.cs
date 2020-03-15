namespace DoenaSoft.WatchHistory.Data
{
    using System;

    internal static class DataManagerFilesExtensions
    {
        internal static DateTime GetCreationTime(this FileEntry entry, IDataManager dataManager)
        {
            if ((entry.CreationTimeValue.HasValue == false) || (entry.CreationTimeValue.Value.Ticks == 0))
            {
                entry.CreationTime = dataManager.GetCreationTime(entry);
            }

            return entry.CreationTime.ToLocalTime();
        }

        internal static uint GetVideoLength(this FileEntry entry, IDataManager dataManager)
        {
            var mediaFileData = dataManager.DetermineVideoLength(entry);

            if (mediaFileData.HasChanged)
            {
                entry.CreationTime = mediaFileData.CreationTime;
                entry.VideoLength = mediaFileData.VideoLength;
            }

            return entry.VideoLength;
        }
    }
}