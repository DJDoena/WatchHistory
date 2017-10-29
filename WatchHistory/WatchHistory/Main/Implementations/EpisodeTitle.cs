﻿using System;
using DoenaSoft.AbstractionLayer.IOServices;
using DoenaSoft.DVDProfiler.DVDProfilerXML.Version390;
using DoenaSoft.WatchHistory.Implementations;

namespace DoenaSoft.WatchHistory.Main.Implementations
{
    internal sealed class EpisodeTitle : IEquatable<EpisodeTitle>
    {
        internal String Title { get; }

        internal DateTime PurchaseDate { get; }

        public EpisodeTitle(DVD dvd
            , String caption
            , IIOServices ioServices)
        {
            PurchaseDate = dvd.PurchaseInfo?.Date ?? new DateTime(0);

            Title = GetTitle(dvd, caption);

            Title = Title.Replace(" :", ":").Replace(":", " -");

            Title = FileNameHelper.GetInstance(ioServices).ReplaceInvalidFileNameChars(Title);
        }

        #region IEquatable<DvdTitle>

        public Boolean Equals(EpisodeTitle other)
        {
            if (other == null)
            {
                return (false);
            }

            Boolean equals = Title.Equals(other.Title);

            if (equals)
            {
                equals = PurchaseDate.Equals(other.PurchaseDate);
            }

            return (equals);
        }

        #endregion

        public override Int32 GetHashCode()
            => ((Title.GetHashCode() / 2) + (PurchaseDate.GetHashCode() / 2));

        public override Boolean Equals(Object obj)
            => (Equals(obj as EpisodeTitle));

        private static string GetTitle(DVD dvd
            , String caption)
            => ($"{GetTitle(dvd)}{Constants.BackSlash}{caption}");

        private static String GetTitle(DVD dvd)
            => (dvd.Title.Replace(": ", Constants.BackSlash));
    }
}