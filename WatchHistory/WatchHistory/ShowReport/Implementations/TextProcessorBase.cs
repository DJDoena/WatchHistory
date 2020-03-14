namespace DoenaSoft.WatchHistory.ShowReport.Implementations
{
    using System;
    using System.Collections.Generic;
    using WatchHistory.Data;

    internal abstract class TextProcessorBase
    {
        protected DateTime Date { get; }

        protected IEnumerable<FileEntry> Entries { get; }

        protected TextProcessorBase(DateTime date, IEnumerable<FileEntry> entries)
        {
            Date = date;
            Entries = entries;
        }

        internal abstract string GetText();
    }
}
