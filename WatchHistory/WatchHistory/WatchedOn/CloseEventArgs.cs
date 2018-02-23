namespace DoenaSoft.WatchHistory.WatchedOn
{
    using System;
    using AbstractionLayer.UIServices;

    internal sealed class CloseEventArgs : EventArgs
    {
        public Result Result { get; }

        public CloseEventArgs(Result result)
            => Result = result;
    }
}