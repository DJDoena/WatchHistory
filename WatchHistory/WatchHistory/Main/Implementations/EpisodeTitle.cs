using System;
using System.Collections.Generic;
using DoenaSoft.ToolBox.Extensions;
using DVDP = DoenaSoft.DVDProfiler.DVDProfilerXML.Version400;

namespace DoenaSoft.WatchHistory.Main.Implementations
{
    internal sealed class EpisodeTitle : IEquatable<EpisodeTitle>
    {
        private readonly DVDP.DVD _dvd;

        internal string ID => _dvd.ID;

        internal string FileName => $"{this.ID}  {this.Title.ReplaceInvalidFileNameChars('_')}";

        internal string Title { get; }

        internal DateTime PurchaseDate => _dvd.PurchaseInfo?.Date ?? new DateTime(0);

        internal IEnumerable<DVDP.Event> Watches => CollectionProcessor.GetWatches(_dvd);

        public EpisodeTitle(DVDP.DVD dvd, string caption)
        {
            _dvd = dvd;

            this.Title = $"{dvd.Title}: {caption}";

            if ((dvd.OriginalTitle.IsNotEmpty()) && (dvd.Title != dvd.OriginalTitle))
            {
                this.Title += $" ({dvd.OriginalTitle})";
            }
        }

        #region IEquatable<DvdTitle>

        public bool Equals(EpisodeTitle other)
        {
            if (other == null)
            {
                return false;
            }

            var equals = this.ID == other.ID;

            if (equals)
            {
                equals = this.Title == other.Title;
            }

            return equals;
        }

        #endregion

        public override int GetHashCode() => this.ID.GetHashCode() ^ this.Title.GetHashCode();

        public override bool Equals(object obj) => this.Equals(obj as EpisodeTitle);
    }
}