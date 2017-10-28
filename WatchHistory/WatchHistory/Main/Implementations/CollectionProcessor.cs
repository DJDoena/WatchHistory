using System;
using System.Collections.Generic;
using System.Linq;
using DoenaSoft.AbstractionLayer.IOServices;
using DoenaSoft.DVDProfiler.DVDProfilerXML.Version390;
using DoenaSoft.ToolBox.Extensions;
using DoenaSoft.WatchHistory.Data;
using DoenaSoft.WatchHistory.Implementations;

namespace DoenaSoft.WatchHistory.Main.Implementations
{
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
                String folder = IOServices.Path.Combine(App.AppDataFolder, "DVDProfiler");

                if (IOServices.Directory.Exists(folder))
                {
                    DeleteProfiles(folder);
                }
                else
                {
                    IOServices.Directory.CreateFolder(folder);
                }

                DataManager.Users = GetUsers().Union(DataManager.Users);

                DataManager.RootFolders = folder.Enumerate().Union(DataManager.RootFolders);

                DataManager.FileExtensions = "dvdp".Enumerate().Union(DataManager.FileExtensions);

                CreateCollectionFiles(folder);
            }
            finally
            {
                DataManager.Resume();
            }
        }

        private void DeleteProfiles(String folder)
        {
            IEnumerable<String> files = IOServices.Directory.GetFiles(folder, "*.dvdp");

            foreach (String file in files)
            {
                IOServices.File.Delete(file);
            }
        }

        private IEnumerable<String> GetUsers()
        {
            IEnumerable<DVD> dvds = Collection.DVDList ?? Enumerable.Empty<DVD>();

            foreach (DVD dvd in dvds)
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
        }
        private void CreateCollectionFiles(String folder)
        {
            IEnumerable<String> titles = GetEpisodeTitles();

            titles = titles.Select(title => title.Replace(" :", ":").Replace(":", " -"));

            titles = titles.Select(FileNameHelper.GetInstance(IOServices).ReplaceInvalidFileNameChars);

            titles = new HashSet<String>(titles);

            foreach (String title in titles)
            {
                String fileName = IOServices.Path.Combine(folder, title + ".dvdp");

                using (System.IO.Stream stream = IOServices.GetFileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.Read))
                { }
            }
        }

        private IEnumerable<String> GetEpisodeTitles()
        {
            IEnumerable<DVD> dvds = Collection.DVDList ?? Enumerable.Empty<DVD>();

            foreach (DVD dvd in dvds)
            {
                IEnumerable<Object> cast = dvd.CastList ?? Enumerable.Empty<Object>();

                foreach (String caption in GetCaptions(cast))
                {
                    yield return ($"{GetTitle(dvd)}{Constants.BackSlash}{caption}");
                }

                IEnumerable<Object> crew = dvd.CrewList ?? Enumerable.Empty<Object>();

                foreach (String caption in GetCaptions(crew))
                {
                    yield return ($"{GetTitle(dvd)}{Constants.BackSlash}{caption}");
                }
            }
        }

        private static String GetTitle(DVD dvd)
            => (dvd.Title.Replace(": ", Constants.BackSlash));

        private IEnumerable<String> GetCaptions(IEnumerable<Object> list)
        {
            IEnumerable<Divider> dividers = list.OfType<Divider>();

            dividers = dividers.Where(div => div?.Type == DividerType.Episode);

            foreach (Divider divider in dividers)
            {
                yield return (divider.Caption);
            }
        }
    }
}