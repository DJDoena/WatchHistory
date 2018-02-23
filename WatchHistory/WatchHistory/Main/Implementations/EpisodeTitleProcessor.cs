namespace DoenaSoft.WatchHistory.Main.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AbstractionLayer.IOServices;
    using DVDProfiler.DVDProfilerXML.Version400;
    using ToolBox.Extensions;

    internal sealed class EpisodeTitleProcessor
    {
        private readonly Collection _Collection;

        private readonly IIOServices _IOServices;

        public EpisodeTitleProcessor(Collection collection
            , IIOServices ioServices)
        {
            _Collection = collection;
            _IOServices = ioServices;
        }

        internal IEnumerable<EpisodeTitle> GetEpisodeTitles()
        {
            IEnumerable<DVD> dvds = _Collection.DVDList.EnsureNotNull();

            IEnumerable<IEnumerable<EpisodeTitle>> castTitles = dvds.Select(dvd => GetEpisodeTitles(dvd, dvd.CastList));

            IEnumerable<IEnumerable<EpisodeTitle>> crewTitles = dvds.Select(dvd => GetEpisodeTitles(dvd, dvd.CrewList));

            IEnumerable<IEnumerable<EpisodeTitle>> castAndCrewTitles = castTitles.Union(crewTitles);

            IEnumerable<EpisodeTitle> titles = castAndCrewTitles.SelectMany(title => title);

            return (titles);
        }

        private IEnumerable<EpisodeTitle> GetEpisodeTitles(DVD dvd
            , IEnumerable<Object> castOrCrew)
        {
            IEnumerable<Divider> dividers = castOrCrew.EnsureNotNull().OfType<Divider>();

            IEnumerable<Divider> episodeDividers = dividers.Where(div => div?.Type == DividerType.Episode);

            IEnumerable<String> captions = episodeDividers.Select(divider => divider.Caption);

            IEnumerable<EpisodeTitle> titles = captions.Select(caption => new EpisodeTitle(dvd, caption, _IOServices));

            return (titles);
        }
    }
}