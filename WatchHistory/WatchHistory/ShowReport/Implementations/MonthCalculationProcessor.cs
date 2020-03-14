namespace DoenaSoft.WatchHistory.ShowReport.Implementations
{
    using System;
    using System.Collections.Generic;
    using WatchHistory.Data;

    internal sealed class MonthCalculationProcessor : CalculationProcessorBase
    {
        internal MonthCalculationProcessor(IDataManager dataManager, string userName, DateTime date) : base(dataManager, userName, date)
        {
        }

        internal override IEnumerable<FileEntry> GetEntries() => GetFilteredEntries();

        protected override bool WatchContainsDate(Watch watch) => WatchHelper.MatchesMonth(watch, Date);
    }
}