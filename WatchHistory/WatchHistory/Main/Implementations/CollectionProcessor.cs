﻿namespace DoenaSoft.WatchHistory.Main.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AbstractionLayer.IOServices;
    using Data;
    using DVDProfiler.DVDProfilerXML.Version390;
    using ToolBox.Extensions;

    internal sealed class CollectionProcessor
    {
        internal const String FileExtension = "dvdp";

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

            DataManager.Users = GetUsers().SelectMany(user => user).Union(DataManager.Users);

            DataManager.RootFolders = folder.Enumerate().Union(DataManager.RootFolders);

            DataManager.FileExtensions = FileExtension.Enumerate().Union(DataManager.FileExtensions);

            CreateCollectionFiles(folder);
        }

        private void DeleteProfiles(String folder)
        {
            IEnumerable<String> files = IOServices.Folder.GetFiles(folder, "*." + FileExtension);

            foreach (String file in files)
            {
                IOServices.File.Delete(file);
            }
        }

        private IEnumerable<IEnumerable<String>> GetUsers()
        {
            IEnumerable<DVD> dvds = Collection.DVDList ?? Enumerable.Empty<DVD>();

            foreach (DVD dvd in dvds)
            {
                yield return (GetUsers(dvd));
            }
        }

        private IEnumerable<String> GetUsers(DVD dvd)
        {
            IEnumerable<Event> events = dvd.EventList ?? Enumerable.Empty<Event>();

            foreach (Event e in events)
            {
                if (e.Type == EventType.Watched)
                {
                    String user = String.Join(" ", e.User?.FirstName, e.User?.LastName).Trim();

                    yield return (user);
                }
            }
        }

        private void CreateCollectionFiles(String folder)
        {
            IEnumerable<EpisodeTitle> titles = (new EpisodeTitleProcessor(Collection, IOServices)).GetEpisodeTitles();

            titles = new HashSet<EpisodeTitle>(titles);

            foreach (EpisodeTitle title in titles)
            {
                CreateCollectionFile(folder, title);
            }
        }

        private void CreateCollectionFile(String folder
            , EpisodeTitle title)
        {
            String fileName = IOServices.Path.Combine(folder, title.Title + "." + FileExtension);

            using (System.IO.Stream stream = IOServices.GetFileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.Read))
            { }

            IFileInfo fi = IOServices.GetFileInfo(fileName);

            fi.CreationTime = title.PurchaseDate;
        }
    }
}