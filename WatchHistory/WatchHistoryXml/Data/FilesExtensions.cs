using System;
using System.Collections.Generic;
using System.Linq;

namespace DoenaSoft.WatchHistory.Data
{
    public static class FilesExtensions
    {
        public static User TryGetUser(this FileEntry entry, string userName) => entry.Users?.FirstOrDefault(user => user.UserName == userName);

        public static IEnumerable<Watch> GetWatches(this FileEntry entry, string userName) => TryGetUser(entry, userName)?.Watches ?? Enumerable.Empty<Watch>();

        public static bool MatchesDay(this Watch watch, DateTime date)
        {
            if (!string.IsNullOrWhiteSpace(watch.Source))
            {
                return false;
            }

            var watchDate = watch.Value.ToLocalTime();

            var dayIsMatch = watchDate.Date == date.Date;

            return dayIsMatch;
        }

        public static bool MatchesMonth(this Watch watch, DateTime date)
        {
            if (!string.IsNullOrWhiteSpace(watch.Source))
            {
                return false;
            }

            var watchDate = watch.Value.ToLocalTime();

            var monthIsMatch = watchDate.Year == date.Year && watchDate.Month == date.Month;

            return monthIsMatch;
        }

        public static bool MatchesYear(this Watch watch, DateTime date)
        {
            if (!string.IsNullOrWhiteSpace(watch.Source))
            {
                return false;
            }

            var watchDate = watch.Value.ToLocalTime();

            var yearIsMatch = watchDate.Year == date.Year;

            return yearIsMatch;
        }

        public static IEnumerable<FileEntry> GetEntriesByUserAndWatchDate(this IEnumerable<FileEntry> entries, string userName, Func<Watch, bool> watchContainsDate)
        {
            var filteredEntries = entries.Where(entry => entry.GetWatchesByUserAndWatchDate(userName, watchContainsDate).Any());

            return filteredEntries;
        }

        public static IEnumerable<Watch> GetWatchesByUserAndWatchDate(this FileEntry entry, string userName, Func<Watch, bool> isValidWatch) => entry.GetWatches(userName).Where(isValidWatch);
    }
}