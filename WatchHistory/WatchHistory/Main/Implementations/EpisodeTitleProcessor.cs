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
            var dvds = _collection.DVDList.EnsureNotNull();

            var castTitles = dvds.Select(dvd => GetEpisodeTitles(dvd, dvd.CastList));

            var crewTitles = dvds.Select(dvd => GetEpisodeTitles(dvd, dvd.CrewList));

            var castAndCrewTitles = castTitles.Union(crewTitles);

            var titles = castAndCrewTitles.SelectMany(title => title);

            return titles;
        }

        private IEnumerable<EpisodeTitle> GetEpisodeTitles(DVD dvd, IEnumerable<object> castOrCrew)
        {
            var dividers = castOrCrew.EnsureNotNull().OfType<Divider>();

            var episodeDividers = dividers.Where(div => div?.Type == DividerType.Episode);

            var captions = episodeDividers.Select(divider => divider.Caption);

            var titles = captions.Select(caption => new EpisodeTitle(dvd, caption));

            return titles;
        }
    }
}