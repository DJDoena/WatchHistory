namespace DoenaSoft.WatchHistory.ShowReport.Implementations
{
    using System;
    using System.Collections.Generic;
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

        protected override bool WatchContainsDate(Watch watch) => watch.MatchesDay(_date);

        private int CompareWatchDates(FileEntry left, FileEntry right)
        {
            var leftLastWatched = _dataManager.GetLastWatched(left, _userName);

            var rightLastWatched = _dataManager.GetLastWatched(right, _userName);

            var compare = leftLastWatched.CompareTo(rightLastWatched);

            return compare;
        }
    }
}