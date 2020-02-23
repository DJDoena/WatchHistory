namespace DoenaSoft.WatchHistory.ShowWatches.Implementations
{
    using Data;
    using WatchHistory.Implementations;

    internal class WatchViewModel : IWatchViewModel
    {
        private readonly Watch _watch;

        public WatchViewModel(Watch watch)
        {
            _watch = watch;
        }

        #region  IWatchViewModel

        public string Source => _watch.SourceSpecified ? _watch.Source : "Watch History";

        public string Watched => ViewModelHelper.GetFormattedDateTime(_watch.Value.ToLocalTime());

        #endregion
    }
}