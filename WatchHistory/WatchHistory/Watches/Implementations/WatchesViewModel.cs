namespace DoenaSoft.WatchHistory.Watches.Implementations
{
    using System.Collections.Generic;
    using System.Linq;
    using Data;

    internal sealed class WatchesViewModel : IWatchesViewModel
    {
        public WatchesViewModel(IEnumerable<Watch> watches)
        {
            IEnumerable<Watch> ordered = watches.OrderByDescending(w => w.Value);

            Watches = ordered.Select(GetWatchViewModel);
        }

        #region IWatchesViewModel

        public IEnumerable<IWatchViewModel> Watches { get; }

        #endregion

        private IWatchViewModel GetWatchViewModel(Watch watch)
            => new WatchViewModel(watch);
    }
}