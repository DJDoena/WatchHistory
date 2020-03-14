﻿namespace DoenaSoft.WatchHistory.ShowReport.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using MediaInfoHelper;
    using WatchHistory.Data;

    internal sealed class MonthTextProcessor : TextProcessorBase
    {
        internal MonthTextProcessor(DateTime date, IEnumerable<FileEntry> entries) : base(date, entries)
        {
        }

        internal override string GetText()
        {
            var totalLength = Entries.Select(e => e.VideoLength).Sum();

            var dailyLength = GetDailyLength(totalLength);

            var text = new StringBuilder();

            text.AppendLine(Date.ToString("MMMM yyyy"));

            text.Append("Total: ");
            text.AppendLine(Helper.FormatTime(totalLength));

            text.Append("Average: ");
            text.Append(Helper.FormatTime(dailyLength));
            text.AppendLine(" per day");

            return text.ToString();
        }

        private uint GetDailyLength(uint totalLength)
        {
            var daysInMonth = DateTime.DaysInMonth(Date.Year, Date.Month);

            var lastDayInMonth = new DateTime(Date.Year, Date.Month, daysInMonth);

            var today = DateTime.Now.Date;

            var days = today > lastDayInMonth ? lastDayInMonth.Day : today.Day;

            var dailyLength = (uint)(totalLength / days);

            return dailyLength;
        }
    }
}