namespace DoenaSoft.WatchHistory.Watches.Implementations
{
    using System;
    using Data;
    using WatchHistory.Implementations;

    internal class WatchViewModel : IWatchViewModel
    {
        private readonly Watch _Watch;

        public WatchViewModel(Watch watch)
        {
            _Watch = watch;
        }

        #region  IWatchViewModel

        public String Source
            => _Watch.SourceSpecified ? _Watch.Source : "Watch History";

        public String Watched
            => ViewModelHelper.GetFormattedDateTime(_Watch.Value.ToLocalTime());

        #endregion
    }
}