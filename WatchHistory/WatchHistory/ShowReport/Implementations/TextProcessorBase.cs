namespace DoenaSoft.WatchHistory.ShowReport.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using WatchHistory.Data;

    internal abstract class TextProcessorBase
    {
        protected DateTime Date { get; }

        protected IEnumerable<FileEntry> Entries { get; }

        protected string UserName { get; }

        protected TextProcessorBase(DateTime date, IEnumerable<FileEntry> entries, string userName)
        {
            Date = date.Date;
            Entries = entries;
            UserName = userName;
        }

        internal abstract string GetText();

        protected abstract bool WatchContainsDate(Watch watch);

        protected uint GetVideoLength(FileEntry entry)
        {
            var watches = entry.GetWatchesByUserAndWatchDate(UserName, WatchContainsDate).ToList();

            var singleLength = entry.VideoLength;

            var fullLength = (uint)(singleLength * watches.Count);

            return fullLength;
        }        
    }
}
