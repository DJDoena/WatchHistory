namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DVDProfiler.DVDProfilerHelper;
    using ToolBox.Extensions;
    using WatchHistory.Main.Implementations;
    using Version390 = DVDProfiler.DVDProfilerXML.Version390;

    internal sealed class DvdWatchesProcessor
    {
        private Dictionary<User, HashSet<Watch>> ExistingWatches { get; set; }

        internal void UpdateFromDvdWatches(FileEntry entry)
        {
            DvdWatches watches = null;
            try
            {
                watches = Serializer<DvdWatches>.Deserialize(entry.FullName);
            }
            catch
            { }

            if (watches?.Watches?.Length > 0)
            {
                UpdateFromDvdWatches(entry, watches);
            }
        }

        private void UpdateFromDvdWatches(FileEntry entry, DvdWatches watches)
        {
            ExistingWatches = new Dictionary<User, HashSet<Watch>>();

            AddExistingWatches(entry);

            UpdateFromDvdWatches(watches);

            List<User> entryUsers = entry.Users.EnsureNotNull().ToList();

            foreach (KeyValuePair<User, HashSet<Watch>> kvp in ExistingWatches)
            {
                UpdateEntryWatches(entryUsers, kvp.Key, kvp.Value);
            }

            entry.Users = (entryUsers.Count > 0) ? entryUsers.ToArray() : null;
        }

        private static void UpdateEntryWatches(List<User> entryUsers, User hashedUser, IEnumerable<Watch> hashedWatches)
        {
            User entryUser = EnsureEntryUser(entryUsers, hashedUser);

            Watch[] entryWatches = hashedWatches.ToArray();

            entryUser.Watches = (entryWatches.Length > 0) ? entryWatches : null;
        }

        private static User EnsureEntryUser(List<User> entryUsers, User hashedUser)
        {
            User entryUser = entryUsers.Find(user => user.Equals(hashedUser));

            if (entryUser == null)
            {
                entryUser = hashedUser;

                entryUsers.Add(entryUser);
            }

            return (entryUser);
        }

        private void UpdateFromDvdWatches(DvdWatches dvdWatches)
        {
            foreach (Version390.Event dvdWatch in dvdWatches.Watches.EnsureNotNull())
            {
                UpdateFromDvdWatch(dvdWatch);
            }
        }

        private void UpdateFromDvdWatch(Version390.Event dvdWatch)
        {
            User user = new User()
            {
                UserName = CollectionProcessor.GetUserName(dvdWatch)
            };

            if (ExistingWatches.TryGetValue(user, out HashSet<Watch> entryWatches) == false)
            {
                entryWatches = new HashSet<Watch>();

                ExistingWatches.Add(user, entryWatches);
            }

            if (entryWatches.HasItemsWhere(entryWatch => DatesAreEqual(entryWatch, dvdWatch.Timestamp)) == false)
            {
                Watch entryWatch = new Watch()
                {
                    Value = dvdWatch.Timestamp.Conform(),
                    Source = Constants.DvdProfilerSource
                };

                entryWatches.Add(entryWatch);
            }
        }

        private static Boolean DatesAreEqual(Watch entryWatch, DateTime dvdWatch)
        {
            DateTime left = entryWatch.Value.ToLocalTime();

            DateTime right = dvdWatch.ToLocalTime();

            if (entryWatch.Source.IsEmpty())
            {
                left = left.Date;

                right = right.Date;
            }

            return (left.Equals(right));
        }

        private void AddExistingWatches(FileEntry entry)
        {
            foreach (User user in entry.Users.EnsureNotNull())
            {
                AddExistingWatches(user);
            }
        }

        private void AddExistingWatches(User user)
        {
            IEnumerable<Watch> watches = user.Watches.EnsureNotNull().Where(w => w.Source != Constants.DvdProfilerSource);

            if (watches.HasItems())
            {
                AddExistingWatches(user, watches);
            }
        }

        private void AddExistingWatches(User user
            , IEnumerable<Watch> watches)
        {
            HashSet<Watch> hashed = new HashSet<Watch>();

            ExistingWatches.Add(user, hashed);

            foreach (Watch watch in watches)
            {
                hashed.Add(watch);
            }
        }
    }
}