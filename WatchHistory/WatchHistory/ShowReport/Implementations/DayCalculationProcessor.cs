namespace DoenaSoft.WatchHistory.ShowReport.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using WatchHistory.Data;

    internal sealed class DayCalculationProcessor : CalculationProcessorBase
    {
        internal DayCalculationProcessor(IDataManager dataManager, string userName, DateTime date) : base(dataManager, userName, date)
        {
        }

        internal override IEnumerable<FileEntry> GetEntries()
        {
            var entries = GetFilteredEntries();

            entries.Sort(CompareWatchDates);

            return entries;
        }

        protected override bool WatchesContainsDate(Watch watch) => string.IsNullOrWhiteSpace(watch.Source) && watch.Value.ToLocalTime().Date == Date;

        private int CompareWatchDates(FileEntry left, FileEntry right)
        {
            var leftLastWatched = GetLastWatched(left);

            var rightLastWatched = GetLastWatched(right);

            var compare = leftLastWatched.CompareTo(rightLastWatched);

            return compare;
        }

        private DateTime GetLastWatched(FileEntry entry) => entry.Users.Max(GetLastWatched);

        private DateTime GetLastWatched(User user)
        {
            if (user.UserName != UserName)
            {
                return new DateTime(0);
            }

            var watches = user.Watches.Where(WatchesContainsDate);

            var lastWatched = watches.Max(watch => watch.Value).ToLocalTime();

            return lastWatched;
        }
    }
}