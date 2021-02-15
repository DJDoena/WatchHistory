namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System.Collections.Generic;
    using System.Linq;
    using AbstractionLayer.IOServices;
    using MediaInfoHelper;
    using ToolBox.Extensions;
    using WatchHistory.Implementations;
    using MIH = MediaInfoHelper.DVDProfiler;

    internal sealed class DvdWatchesProcessor
    {
        private readonly IIOServices _ioServices;

        private Dictionary<User, HashSet<Watch>> ExistingWatches { get; set; }

        public DvdWatchesProcessor(IIOServices ioServices)
        {
            _ioServices = ioServices;
        }

        internal void Update(FileEntry entry)
        {
            var fi = _ioServices.GetFileInfo(entry.FullName);

            if (!fi.Exists)
            {
                return;
            }

            entry.CreationTime = fi.CreationTimeUtc.Conform();

            MIH.DvdWatches watches = null;
            try
            {
                watches = SerializerHelper.Deserialize<MIH.DvdWatches>(_ioServices, entry.FullName);
            }
            catch
            { }

            if (watches == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(entry.Title))
            {
                entry.Title = watches.Title;
            }

            if (watches.Watches?.Length > 0)
            {
                UpdateFromDvdWatches(entry, watches.Watches);
            }
        }

        private void UpdateFromDvdWatches(FileEntry entry, IEnumerable<MIH.Event> dvdWatches)
        {
            ExistingWatches = new Dictionary<User, HashSet<Watch>>();

            var entryUsers = entry.Users.EnsureNotNull().ToList();

            entryUsers.ForEach(AddExistingWatches);

            dvdWatches.ForEach(UpdateFromDvdWatch);

            ExistingWatches.ForEach(kvp => UpdateEntryWatches(entryUsers, kvp.Key, kvp.Value));

            entry.Users = entryUsers.Count > 0
                ? entryUsers.ToArray()
                : null;
        }

        private static void UpdateEntryWatches(List<User> entryUsers, User hashedUser, IEnumerable<Watch> hashedWatches)
        {
            var entryUser = EnsureEntryUser(entryUsers, hashedUser);

            var entryWatches = hashedWatches.ToArray();

            entryUser.Watches = entryWatches.Length > 0
                ? entryWatches
                : null;
        }

        private static User EnsureEntryUser(List<User> entryUsers, User hashedUser)
        {
            var entryUser = entryUsers.Find(user => user.Equals(hashedUser));

            if (entryUser == null)
            {
                entryUser = hashedUser;

                entryUsers.Add(entryUser);
            }

            return entryUser;
        }

        private void UpdateFromDvdWatch(MIH.Event dvdWatch)
        {
            var user = new User()
            {
                UserName = string.Join(" ", dvdWatch.User?.FirstName, dvdWatch.User?.LastName).Trim(),
            };

            if (ExistingWatches.TryGetValue(user, out HashSet<Watch> entryWatches) == false)
            {
                entryWatches = new HashSet<Watch>();

                ExistingWatches.Add(user, entryWatches);
            }

            var entryWatch = new Watch()
            {
                Value = dvdWatch.Timestamp.Conform(),
                Source = WatchHistory.Constants.DvdProfilerSource
            };

            entryWatches.Add(entryWatch);
        }

        private void AddExistingWatches(User user)
        {
            var watches = user.Watches.EnsureNotNull().Where(w => w.Source != WatchHistory.Constants.DvdProfilerSource);

            if (watches.HasItems())
            {
                AddExistingWatches(user, watches);
            }
        }

        private void AddExistingWatches(User user, IEnumerable<Watch> watches)
        {
            var hashed = new HashSet<Watch>();

            ExistingWatches.Add(user, hashed);

            watches.ForEach(watch => hashed.Add(watch));
        }
    }
}