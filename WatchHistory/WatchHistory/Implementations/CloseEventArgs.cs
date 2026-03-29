using System;
using DoenaSoft.AbstractionLayer.UIServices;

namespace DoenaSoft.WatchHistory.Implementations;

internal sealed class CloseEventArgs : EventArgs
{
    public MessageResult Result { get; }

    public CloseEventArgs(MessageResult result) => this.Result = result;
}