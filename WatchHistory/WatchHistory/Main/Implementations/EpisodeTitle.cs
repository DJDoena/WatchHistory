namespace DoenaSoft.WatchHistory.Main.Implementations
{
    using System;
    using System.Collections.Generic;
    using AbstractionLayer.IOServices;
    using DVDProfiler.DVDProfilerXML.Version400;
    using ToolBox.Extensions;

    internal sealed class EpisodeTitle : IEquatable<EpisodeTitle>
    {
        private readonly DVD _Dvd;

        internal String ID
            => _Dvd.ID;

        internal String FileName
            => $"{ID}  {Title.ReplaceInvalidFileNameChars('_')}";

        internal String Title { get; }

        internal DateTime PurchaseDate
            => _Dvd.PurchaseInfo?.Date ?? new DateTime(0);

        internal IEnumerable<Event> Watches
            => CollectionProcessor.GetWatches(_Dvd);

        public EpisodeTitle(DVD dvd
            , String caption
            , IIOServices ioServices)
        {
            _Dvd = dvd;

            Title = $"{dvd.Title}: {caption}";

            if ((dvd.OriginalTitle.IsNotEmpty()) && (dvd.Title != dvd.OriginalTitle))
            {
                Title += $" ({dvd.Title})";
            }
        }

        #region IEquatable<DvdTitle>

        public Boolean Equals(EpisodeTitle other)
        {
            if (other == null)
            {
                return (false);
            }

            Boolean equals = ID == other.ID;

            if (equals)
            {
                equals = Title == other.Title;
            }

            return (equals);
        }

        #endregion

        public override Int32 GetHashCode()
            => ((ID.GetHashCode() / 2) + (Title.GetHashCode() / 2));

        public override Boolean Equals(Object obj)
            => (Equals(obj as EpisodeTitle));
    }
}