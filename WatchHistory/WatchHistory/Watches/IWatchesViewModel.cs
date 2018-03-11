namespace DoenaSoft.WatchHistory.Watches
{
    using System.Collections.Generic;

    internal interface IWatchesViewModel
    {
        IEnumerable<IWatchViewModel> Watches { get; }
    }
}