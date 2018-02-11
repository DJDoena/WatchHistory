namespace DoenaSoft.WatchHistory.Main.Implementations
{
    using System;
    using System.Collections.Generic;
    using AbstractionLayer.IOServices;
    using DVDProfiler.DVDProfilerXML.Version400;
    using ToolBox.Extensions;

    internal sealed class EpisodeTitle : IEquatable<EpisodeTitle>
    {
        private readonly DVD Dvd;

        internal String Title { get; }

        internal DateTime PurchaseDate 
            => Dvd.PurchaseInfo?.Date ?? new DateTime(0);

        internal IEnumerable<Event> Watches
            => CollectionProcessor.GetWatches(Dvd);

        public EpisodeTitle(DVD dvd
            , String caption
            , IIOServices ioServices)
        {
            Dvd = dvd;

            Title = GetTitle(dvd, caption);

            Title = Title.Replace(" :", ":").Replace(":", " -");

            Title = Title.ReplaceInvalidFileNameChars(' ');
        }

        #region IEquatable<DvdTitle>

        public Boolean Equals(EpisodeTitle other)
        {
            if (other == null)
            {
                return (false);
            }

            Boolean equals = Dvd.ID == other.Dvd.ID;

            if (equals)
            {
                equals = Title == other.Title;
            }

            return (equals);
        }

        #endregion

        public override Int32 GetHashCode()
            => ((Dvd.ID.GetHashCode() / 2) + (Title.GetHashCode() / 2));

        public override Boolean Equals(Object obj)
            => (Equals(obj as EpisodeTitle));

        private static string GetTitle(DVD dvd
            , String caption)
            => ($"{GetTitle(dvd)}{Constants.Backslash}{caption}");

        private static String GetTitle(DVD dvd)
            => (dvd.Title.Replace(": ", Constants.Backslash));
    }
}