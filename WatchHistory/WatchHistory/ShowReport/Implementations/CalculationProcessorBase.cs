namespace DoenaSoft.WatchHistory.ShowReport.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MediaInfoHelper;
    using WatchHistory.Data;

    internal abstract class CalculationProcessorBase
    {
        private readonly IDataManager _dataManager;

        protected string UserName { get; }

        protected DateTime Date { get; }

        protected bool EntryContainsUserWithWatchedDate(FileEntry entry) => entry.Users?.Any(UserIsCorrectAndContainsDate) == true;

        protected abstract bool WatchesContainsDate(Watch watch);

        protected CalculationProcessorBase(IDataManager dataManager, string userName, DateTime date)
        {
            _dataManager = dataManager;
            UserName = userName;
            Date = date.Date;
        }

        internal abstract IEnumerable<FileEntry> GetEntries();

        protected List<FileEntry> GetFilteredEntries()
        {
            var entries = _dataManager.GetFiles().Where(EntryContainsUserWithWatchedDate).ToList();

            foreach (var entry in entries)
            {
                var mediaFileData = new MediaFileData(entry.FullName, entry.CreationTime, entry.VideoLength);

                var hasChanged = (new VideoReader(mediaFileData, false)).DetermineLength();

                if (hasChanged)
                {
                    entry.VideoLength = mediaFileData.VideoLength;
                    entry.CreationTime = mediaFileData.CreationTime;
                }
            }

            return entries;
        }

        private bool UserIsCorrectAndContainsDate(User user) => user.UserName == UserName && user.Watches?.Any(WatchesContainsDate) == true;
    }
}