namespace DoenaSoft.WatchHistory.ShowWatches
{
    using System.Collections.Generic;

    internal interface IShowWatchesViewModel
    {
        IEnumerable<IWatchViewModel> Watches { get; }
    }
}