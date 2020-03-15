namespace DoenaSoft.WatchHistory.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class FilesExtensions
    {
        internal static User TryGetUser(this FileEntry entry, string userName) => entry.Users?.FirstOrDefault(user => user.UserName == userName);

        internal static IEnumerable<Watch> GetWatches(this FileEntry entry, string userName) => TryGetUser(entry, userName)?.Watches ?? Enumerable.Empty<Watch>();

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

        internal static bool MatchesYear(this Watch watch, DateTime date)
        {
            if (!string.IsNullOrWhiteSpace(watch.Source))
            {
                return false;
            }

            var watchDate = watch.Value.ToLocalTime();

            var yearIsMatch = watchDate.Year == date.Year;

            return yearIsMatch;
        }

        internal static IEnumerable<FileEntry> GetEntriesByUserAndWatchDate(this IEnumerable<FileEntry> entries, string userName, Func<Watch, bool> watchContainsDate)
        {
            var filteredEntries = entries.Where(entry => entry.GetWatchesByUserAndWatchDate(userName, watchContainsDate).Any());

            return filteredEntries;
        }

        internal static IEnumerable<Watch> GetWatchesByUserAndWatchDate(this FileEntry entry, string userName, Func<Watch, bool> isValidWatch) => entry.GetWatches(userName).Where(isValidWatch);
    }
}