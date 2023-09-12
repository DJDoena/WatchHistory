using System.Collections.Generic;
using System.Linq;
using DoenaSoft.ToolBox.Extensions;
using DVDP = DoenaSoft.DVDProfiler.DVDProfilerXML.Version400;

namespace DoenaSoft.WatchHistory.Main.Implementations
{
    internal sealed class EpisodeTitleProcessor
    {
        private readonly DVDP.Collection _collection;

        public EpisodeTitleProcessor(DVDP.Collection collection)
        {
            _collection = collection;
        }

        internal IEnumerable<EpisodeTitle> GetEpisodeTitles()
        {
            var dvds = _collection.DVDList.EnsureNotNull();

            var castTitles = dvds.Select(dvd => this.GetEpisodeTitles(dvd, dvd.CastList));

            var crewTitles = dvds.Select(dvd => this.GetEpisodeTitles(dvd, dvd.CrewList));

            var castAndCrewTitles = castTitles.Union(crewTitles);

            var titles = castAndCrewTitles.SelectMany(title => title);

            return titles;
        }

        private IEnumerable<EpisodeTitle> GetEpisodeTitles(DVDP.DVD dvd, IEnumerable<object> castOrCrew)
        {
            var dividers = castOrCrew.EnsureNotNull().OfType<DVDP.Divider>();

            var episodeDividers = dividers.Where(div => div?.Type == DVDP.DividerType.Episode);

            var captions = episodeDividers.Select(divider => divider.Caption);

            var titles = captions.Select(caption => new EpisodeTitle(dvd, caption));

            return titles;
        }
    }
}