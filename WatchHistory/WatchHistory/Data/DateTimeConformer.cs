namespace DoenaSoft.WatchHistory.Data
{
    using System;

    internal static class DateTimeConformer
    {
        internal static DateTime Conform(this DateTime value)
            => (value.AddTicks(-(value.Ticks % TimeSpan.TicksPerSecond)).ToUniversalTime());
    }
}