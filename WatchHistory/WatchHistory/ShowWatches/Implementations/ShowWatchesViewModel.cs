namespace DoenaSoft.WatchHistory.ShowWatches.Implementations
{
    using System.Collections.Generic;
    using System.Linq;
    using Data;

    internal sealed class ShowWatchesViewModel : IShowWatchesViewModel
    {
        public ShowWatchesViewModel(IEnumerable<Watch> watches)
        {
            var ordered = watches.OrderByDescending(w => w.Value);

            Watches = ordered.Select(GetWatchViewModel);
        }

        #region IShowWatchesViewModel

        public IEnumerable<IWatchViewModel> Watches { get; }

        #endregion

        private IWatchViewModel GetWatchViewModel(Watch watch) => new WatchViewModel(watch);
    }
}