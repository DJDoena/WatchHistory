namespace DoenaSoft.WatchHistory.Data
{
    using System;
    using DoenaSoft.WatchHistory.Data.Implementations;

    partial class FileEntry
    {
        internal DateTime GetCreationTime(IDataManager dataManager)
        {
            if ((m_CreationTime.HasValue == false) || (m_CreationTime.Value.Ticks == 0))
            {
                m_CreationTime = dataManager.GetCreationTime(this).Conform();
            }

            return (m_CreationTime.Value.ToLocalTime());
        }

        internal uint GetVideoLength(IDataManager dataManager)
        {
            if (VideoLengthSpecified == false)
            {
                VideoLength = dataManager.GetVideoLength(this);
            }

            return VideoLength;
        }
    }
}