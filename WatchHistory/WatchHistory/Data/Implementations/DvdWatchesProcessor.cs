namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System.Collections.Generic;
    using System.Linq;
    using AbstractionLayer.IOServices;
    using MediaInfoHelper;
    using ToolBox.Extensions;
    using WatchHistory.Implementations;
    using WatchHistory.Main.Implementations;
    using DVDP = DVDProfiler.DVDProfilerXML.Version400;
    using MIH = MediaInfoHelper.DVDProfiler;

    internal sealed class DvdWatchesProcessor
    {
        private readonly IIOServices _IOServices;

        private Dictionary<User, HashSet<Watch>> ExistingWatches { get; set; }

        public DvdWatchesProcessor(IIOServices ioServices)
        {
            _IOServices = ioServices;
        }

        internal void Update(FileEntry entry)
        {
            MIH.DvdWatches watches = null;

            if (_IOServices.File.Exists(entry.FullName))
            {
                try
                {
                    watches = SerializerHelper.Deserialize<MIH.DvdWatches>(_IOServices, entry.FullName);
                }
                catch
                { }
            }

            entry.Title = watches?.Title;

            if (watches?.Watches?.Length > 0)
            {
                UpdateFromDvdWatches(entry, watches);
            }
        }

        private void UpdateFromDvdWatches(FileEntry entry, MIH.DvdWatches watches)
        {
            ExistingWatches = new Dictionary<User, HashSet<Watch>>();

            var entryUsers = entry.Users.EnsureNotNull().ToList();

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

            Watch entryWatch = new Watch()
            {
                Value = dvdWatch.Timestamp.Conform(),
                Source = WatchHistory.Constants.DvdProfilerSource
            };

            entryWatches.Add(entryWatch);
        }

        private void AddExistingWatches(Data.User user)
        {
            IEnumerable<Watch> watches = user.Watches.EnsureNotNull().Where(w => w.Source != WatchHistory.Constants.DvdProfilerSource);

            if (watches.HasItems())
            {
                AddExistingWatches(user, watches);
            }
        }

        private void AddExistingWatches(Data.User user, IEnumerable<Watch> watches)
        {
            HashSet<Watch> hashed = new HashSet<Watch>();

            ExistingWatches.Add(user, hashed);

            watches.ForEach(watch => hashed.Add(watch));
        }
    }
}