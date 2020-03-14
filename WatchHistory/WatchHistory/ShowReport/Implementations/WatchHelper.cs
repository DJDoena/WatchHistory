namespace DoenaSoft.WatchHistory.ShowReport.Implementations
{
    using System;
    using WatchHistory.Data;

    internal static class WatchHelper
    {
        internal static bool MatchesDay(this Watch watch, DateTime date)
        {
            if (!string.IsNullOrWhiteSpace(watch.Source))
            {
                return false;
            }

            var watchDate = watch.Value.ToLocalTime();

            var dayIsMatch = watchDate.Date == date.Date;

            return dayIsMatch;
        }

        internal static bool MatchesMonth(this Watch watch, DateTime date)
        {
            if (!string.IsNullOrWhiteSpace(watch.Source))
            {
                return false;
            }

            var watchDate = watch.Value.ToLocalTime();

            var monthIsMatch = watchDate.Year == date.Year && watchDate.Month == date.Month;

            return monthIsMatch;
        }
    }
}
