namespace DoenaSoft.WatchHistory.Watches
{
    using System;

    internal interface IWatchViewModel
    {
        String Source { get; }

        String Watched { get; }
    }
}