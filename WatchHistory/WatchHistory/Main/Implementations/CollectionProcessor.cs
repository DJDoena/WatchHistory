namespace DoenaSoft.WatchHistory.Main.Implementations
{
    using System.Collections.Generic;
    using System.Linq;
    using AbstractionLayer.IOServices;
    using Data;
    using ToolBox.Extensions;
    using WatchHistory.Implementations;
    using DVDP = DVDProfiler.DVDProfilerXML.Version400;
    using MIH = MediaInfoHelper.DVDProfiler;

    internal sealed class CollectionProcessor
    {
        private readonly DVDP.Collection _collection;

        private readonly IDataManager _dataManager;

        private readonly IIOServices _ioServices;

        public CollectionProcessor(DVDP.Collection collection, IDataManager dataManager, IIOServices ioServices)
        {
            _collection = collection;
            _dataManager = dataManager;
            _ioServices = ioServices;
        }

        public void Process()
        {
            _dataManager.Suspend();

            try
            {
                TryProcess();
            }
            finally
            {
                _dataManager.Resume();
            }
        }

        private void TryProcess()
        {
            var folder = _ioServices.Path.Combine(Environment.AppDataFolder, "DVDProfiler");

            if (_ioServices.Folder.Exists(folder) == false)
            {
                _ioServices.Folder.CreateFolder(folder);
            }

            _dataManager.Users = GetUsers().Union(_dataManager.Users);

            _dataManager.RootFolders = folder.Enumerate().Union(_dataManager.RootFolders);

            _dataManager.FileExtensions = MediaInfoHelper.Constants.DvdProfilerFileExtensionName.Enumerate().Union(_dataManager.FileExtensions);

            CreateCollectionFiles(folder);
        }

        private IEnumerable<string> GetUsers()
        {
            var dvds = _collection.DVDList.EnsureNotNull();

            var usersByDvd = dvds.Select(GetUsers);

            var users = usersByDvd.SelectMany(user => user);

            return users;
        }

        private IEnumerable<string> GetUsers(DVDP.DVD dvd)
        {
            var watches = GetWatches(dvd);

            var users = watches.Select(GetUserName);

            return users;
        }

        internal static IEnumerable<DVDP.Event> GetWatches(DVDP.DVD dvd) => dvd.EventList.EnsureNotNull().Where(e => e.Type == DVDP.EventType.Watched);

        internal static string GetUserName(DVDP.Event watch) => string.Join(" ", watch.User?.FirstName, watch.User?.LastName).Trim();

        private void CreateCollectionFiles(string folder)
        {
            var titles = (new EpisodeTitleProcessor(_collection)).GetEpisodeTitles();

            titles = new HashSet<EpisodeTitle>(titles);

            titles.ForEach(title => CreateCollectionFile(folder, title));
        }

        private void CreateCollectionFile(string folder, EpisodeTitle title)
        {
            var fileName = _ioServices.Path.Combine(folder, title.FileName + MediaInfoHelper.Constants.DvdProfilerFileExtension);

            var watches = new MIH.DvdWatches()
            {
                Title = title.Title,
                Watches = title.Watches?.Select(ToDvdWatch).ToArray()
            };

            SerializerHelper.Serialize(_ioServices, fileName, watches);

            var fi = _ioServices.GetFileInfo(fileName);

            fi.CreationTime = title.PurchaseDate.Date;

            var fileEntry = new FileEntry()
            {
                FullName = fileName,
                Title = title.Title,
            };

            _dataManager.TryCreateEntry(fileEntry);
        }

        private static MIH.Event ToDvdWatch(DVDP.Event source)
        {
            var target = new MIH.Event()
            {
                Note = source.Note,
                Timestamp = source.Timestamp,
                Type = (MIH.EventType)source.Type,
                User = new MIH.User()
                {
                    EmailAddress = source.User?.EmailAddress,
                    FirstName = source.User?.FirstName,
                    LastName = source.User?.LastName,
                    PhoneNumber = source.User?.PhoneNumber,
                },
            };

            return target;
        }
    }
}