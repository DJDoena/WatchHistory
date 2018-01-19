namespace DoenaSoft.WatchHistory.Data
{
    using System;

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
    }
}