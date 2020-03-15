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

        internal DayTextProcessor(IIOServices ioServices, DateTime date, IEnumerable<FileEntry> entries, string userName) : base(date, entries, userName)
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

            var totalLength = Entries.Select(GetVideoLength).Sum();

            text.Append("Total: ");
            text.AppendLine(Helper.FormatTime(totalLength));

            return text.ToString();
        }

        protected override bool WatchContainsDate(Watch watch) => watch.MatchesDay(Date);

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