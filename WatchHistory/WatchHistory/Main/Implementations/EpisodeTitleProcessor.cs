namespace DoenaSoft.WatchHistory.Main.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AbstractionLayer.IOServices;
    using DVDProfiler.DVDProfilerXML.Version390;
    using ToolBox.Extensions;

    internal sealed class EpisodeTitleProcessor
    {
        private readonly Collection Collection;

        private readonly IIOServices IOServices;

        public EpisodeTitleProcessor(Collection collection
            , IIOServices ioServices)
        {
            Collection = collection;
            IOServices = ioServices;
        }

        internal IEnumerable<EpisodeTitle> GetEpisodeTitles()
            => (GetTitles().SelectMany(title => title));

        private IEnumerable<IEnumerable<EpisodeTitle>> GetTitles()
        {
            IEnumerable<DVD> dvds = Collection.DVDList.EnsureNotNull();

            foreach (DVD dvd in dvds)
            {
                yield return (GetEpisodeTitles(dvd, dvd.CastList));

                yield return (GetEpisodeTitles(dvd, dvd.CrewList));
            }
        }

        private IEnumerable<EpisodeTitle> GetEpisodeTitles(DVD dvd
            , IEnumerable<Object> list)
        {
            IEnumerable<Divider> dividers = GetDividers(list);

            IEnumerable<EpisodeTitle> titles = GetEpisodeTitles(dvd, dividers);

            return (titles);
        }

        private static IEnumerable<Divider> GetDividers(IEnumerable<Object> list)
            => (list.EnsureNotNull().OfType<Divider>());

        private IEnumerable<EpisodeTitle> GetEpisodeTitles(DVD dvd
            , IEnumerable<Divider> dividers)
        {
            foreach (String caption in GetCaptions(dividers))
            {
                yield return (new EpisodeTitle(dvd, caption, IOServices));
            }
        }

        private static IEnumerable<String> GetCaptions(IEnumerable<Divider> dividers)
        {
            dividers = dividers.Where(div => div?.Type == DividerType.Episode);

            foreach (Divider divider in dividers)
            {
                yield return (divider.Caption);
            }
        }
    }
}