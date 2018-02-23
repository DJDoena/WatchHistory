namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AbstractionLayer.IOServices;
    using ToolBox.Extensions;
    using WatchHistory.Implementations;
    using WatchHistory.Main.Implementations;
    using Version400 = DVDProfiler.DVDProfilerXML.Version400;

    internal sealed class DvdWatchesProcessor
    {
        private readonly IIOServices _IOServices;

        private Dictionary<User, HashSet<Watch>> ExistingWatches { get; set; }

        public DvdWatchesProcessor(IIOServices ioServices)
        {
            _IOServices = ioServices;
        }

        internal void UpdateFromDvdWatches(FileEntry entry)
        {
            DvdWatches watches = null;
            try
            {
                watches = SerializerHelper.Deserialize<DvdWatches>(_IOServices, entry.FullName);
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

            List<User> entryUsers = entry.Users.EnsureNotNull().ToList();

            entryUsers.ForEach(AddExistingWatches);

            watches.Watches.EnsureNotNull().ForEach(UpdateFromDvdWatch);

            ExistingWatches.ForEach(kvp => UpdateEntryWatches(entryUsers, kvp.Key, kvp.Value));

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

        private void UpdateFromDvdWatch(Version400.Event dvdWatch)
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

            watches.ForEach(watch => hashed.Add(watch));
        }
    }
}