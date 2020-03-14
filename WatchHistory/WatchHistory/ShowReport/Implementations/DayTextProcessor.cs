namespace DoenaSoft.WatchHistory.ShowReport.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AbstractionLayer.IOServices;
    using MediaInfoHelper;
    using WatchHistory.Data;

    internal sealed class DayTextProcessor : TextProcessorBase
    {
        private readonly IIOServices _ioServices;

        internal DayTextProcessor(IIOServices ioServices, DateTime date, IEnumerable<FileEntry> entries) : base(date, entries)
        {
            _ioServices = ioServices;
        }

        internal override string GetText()
        {
            var text = new StringBuilder();

            text.AppendLine(Date.ToShortDateString());

            foreach (var entry in Entries)
            {
                text.AppendLine(GetTitle(entry));
            }

            var totalLength = Entries.Select(e => e.VideoLength).Sum();

            text.Append("Total: ");
            text.AppendLine(Helper.FormatTime(totalLength));

            return text.ToString();
        }

        private string GetTitle(FileEntry entry)
        {
            if (entry.TitleSpecified)
            {
                return entry.Title;
            }
            else
            {
                var title = _ioServices.GetFileInfo(entry.FullName).NameWithoutExtension;

                return title;
            }
        }
    }
}