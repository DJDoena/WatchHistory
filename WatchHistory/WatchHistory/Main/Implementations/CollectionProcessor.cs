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
        private readonly Collection Collection;

        private readonly IDataManager DataManager;

        private readonly IIOServices IOServices;

        public CollectionProcessor(Collection collection
            , IDataManager dataManager
            , IIOServices ioServices)
        {
            Collection = collection;
            DataManager = dataManager;
            IOServices = ioServices;
        }

        public void Process()
        {
            DataManager.Suspend();

            try
            {
                TryProcess();
            }
            finally
            {
                DataManager.Resume();
            }
        }

        private void TryProcess()
        {
            String folder = IOServices.Path.Combine(WatchHistory.Environment.AppDataFolder, "DVDProfiler");

            if (IOServices.Folder.Exists(folder))
            {
                DeleteProfiles(folder);
            }
            else
            {
                IOServices.Folder.CreateFolder(folder);
            }

            DataManager.Users = GetUsers().Union(DataManager.Users);

            DataManager.RootFolders = folder.Enumerate().Union(DataManager.RootFolders);

            DataManager.FileExtensions = Constants.DvdProfilerFileExtension.Enumerate().Union(DataManager.FileExtensions);

            CreateCollectionFiles(folder);
        }

        private void DeleteProfiles(String folder)
        {
            IEnumerable<String> files = IOServices.Folder.GetFiles(folder, "*." + Constants.DvdProfilerFileExtension);

            files.ForEach(file => IOServices.File.Delete(file));
        }

        private IEnumerable<String> GetUsers()
        {
            IEnumerable<DVD> dvds = Collection.DVDList.EnsureNotNull();

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
            IEnumerable<EpisodeTitle> titles = (new EpisodeTitleProcessor(Collection, IOServices)).GetEpisodeTitles();

            titles = new HashSet<EpisodeTitle>(titles);

            titles.ForEach(title => CreateCollectionFile(folder, title));
        }

        private void CreateCollectionFile(String folder
            , EpisodeTitle title)
        {
            String fileName = IOServices.Path.Combine(folder, title.Title + "." + Constants.DvdProfilerFileExtension);

            DvdWatches watches = new DvdWatches()
            {
                Watches = title.Watches?.ToArray()
            };

            SerializerHelper.Serialize(IOServices, fileName, watches);

            IFileInfo fi = IOServices.GetFileInfo(fileName);

            fi.CreationTime = title.PurchaseDate;
        }
    }
}