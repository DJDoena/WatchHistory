namespace DoenaSoft.WatchHistory.Watches.Implementations
{
    using System.Collections.Generic;
    using System.Linq;
    using Data;

    internal sealed class WatchesViewModel : IWatchesViewModel
    {
        private readonly IEnumerable<IWatchViewModel> _Watches;

        public WatchesViewModel(IEnumerable<Watch> watches)
        {
            IEnumerable<Watch> ordered = watches.OrderByDescending(w => w.Value);

            _Watches = ordered.Select(GetWatchViewModel);
        }

        #region IWatchesViewModel

        public IEnumerable<IWatchViewModel> Watches
            => _Watches;

        #endregion

        private IWatchViewModel GetWatchViewModel(Watch watch)
            => new WatchViewModel(watch);
    }
}