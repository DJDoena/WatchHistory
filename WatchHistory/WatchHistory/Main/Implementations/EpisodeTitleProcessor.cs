namespace DoenaSoft.WatchHistory.Main.Implementations
{
    using System.Collections.Generic;
    using System.Linq;
    using DVDProfiler.DVDProfilerXML.Version400;
    using ToolBox.Extensions;

    internal sealed class EpisodeTitleProcessor
    {
        private readonly Collection _collection;

        public EpisodeTitleProcessor(Collection collection)
        {
            _collection = collection;
        }

        internal IEnumerable<EpisodeTitle> GetEpisodeTitles()
        {
            IEnumerable<DVD> dvds = _collection.DVDList.EnsureNotNull();

            IEnumerable<IEnumerable<EpisodeTitle>> castTitles = dvds.Select(dvd => GetEpisodeTitles(dvd, dvd.CastList));

            IEnumerable<IEnumerable<EpisodeTitle>> crewTitles = dvds.Select(dvd => GetEpisodeTitles(dvd, dvd.CrewList));

            IEnumerable<IEnumerable<EpisodeTitle>> castAndCrewTitles = castTitles.Union(crewTitles);

            IEnumerable<EpisodeTitle> titles = castAndCrewTitles.SelectMany(title => title);

            return (titles);
        }

        private IEnumerable<EpisodeTitle> GetEpisodeTitles(DVD dvd
            , IEnumerable<object> castOrCrew)
        {
            IEnumerable<Divider> dividers = castOrCrew.EnsureNotNull().OfType<Divider>();

            IEnumerable<Divider> episodeDividers = dividers.Where(div => div?.Type == DividerType.Episode);

            IEnumerable<string> captions = episodeDividers.Select(divider => divider.Caption);

            IEnumerable<EpisodeTitle> titles = captions.Select(caption => new EpisodeTitle(dvd, caption));

            return (titles);
        }
    }
}