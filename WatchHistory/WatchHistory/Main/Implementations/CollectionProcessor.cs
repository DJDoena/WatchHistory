namespace DoenaSoft.WatchHistory.Main.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AbstractionLayer.IOServices;
    using Data;
    using DVDProfiler.DVDProfilerXML.Version400;
    using ToolBox.Extensions;
    using WatchHistory.Implementations;

    internal sealed class CollectionProcessor
    {
        private readonly Collection _Collection;

        private readonly IDataManager _DataManager;

        private readonly IIOServices _IOServices;

        public CollectionProcessor(Collection collection
            , IDataManager dataManager
            , IIOServices ioServices)
        {
            _Collection = collection;
            _DataManager = dataManager;
            _IOServices = ioServices;
        }

        public void Process()
        {
            _DataManager.Suspend();

            try
            {
                TryProcess();
            }
            finally
            {
                _DataManager.Resume();
            }
        }

        private void TryProcess()
        {
            String folder = _IOServices.Path.Combine(WatchHistory.Environment.AppDataFolder, "DVDProfiler");

            if (_IOServices.Folder.Exists(folder))
            {
                DeleteProfiles(folder);
            }
            else
            {
                _IOServices.Folder.CreateFolder(folder);
            }

            _DataManager.Users = GetUsers().Union(_DataManager.Users);

            _DataManager.RootFolders = folder.Enumerate().Union(_DataManager.RootFolders);

            _DataManager.FileExtensions = Constants.DvdProfilerFileExtension.Enumerate().Union(_DataManager.FileExtensions);

            CreateCollectionFiles(folder);
        }

        private void DeleteProfiles(String folder)
        {
            IEnumerable<String> files = _IOServices.Folder.GetFiles(folder, "*." + Constants.DvdProfilerFileExtension);

            files.ForEach(file => _IOServices.File.Delete(file));
        }

        private IEnumerable<String> GetUsers()
        {
            IEnumerable<DVD> dvds = _Collection.DVDList.EnsureNotNull();

            IEnumerable<IEnumerable<String>> usersByDvd = dvds.Select(GetUsers);

            IEnumerable<String> users = usersByDvd.SelectMany(user => user);

            return (users);
        }

        private IEnumerable<String> GetUsers(DVD dvd)
        {
            IEnumerable<Event> watches = GetWatches(dvd);

            IEnumerable<String> users = watches.Select(GetUserName);

            return (users);
        }

        internal static IEnumerable<Event> GetWatches(DVD dvd)
            => dvd.EventList.EnsureNotNull().Where(e => e.Type == EventType.Watched);

        internal static String GetUserName(Event watch)
            => String.Join(" ", watch.User?.FirstName, watch.User?.LastName).Trim();

        private void CreateCollectionFiles(String folder)
        {
            IEnumerable<EpisodeTitle> titles = (new EpisodeTitleProcessor(_Collection, _IOServices)).GetEpisodeTitles();

            titles = new HashSet<EpisodeTitle>(titles);

            titles.ForEach(title => CreateCollectionFile(folder, title));
        }

        private void CreateCollectionFile(String folder
            , EpisodeTitle title)
        {
            String fileName = _IOServices.Path.Combine(folder, title.FileName + "." + Constants.DvdProfilerFileExtension);

            DvdWatches watches = new DvdWatches()
            {
                Title = title.Title,
                Watches = title.Watches?.ToArray()
            };

            SerializerHelper.Serialize(_IOServices, fileName, watches);

            IFileInfo fi = _IOServices.GetFileInfo(fileName);

            fi.CreationTime = title.PurchaseDate;
        }
    }
}