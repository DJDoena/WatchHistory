﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DoenaSoft.MediaInfoHelper.Helpers;
using DoenaSoft.WatchHistory.Data;

namespace DoenaSoft.WatchHistory.ShowReport.Implementations
{
    internal sealed class MonthTextProcessor : TextProcessorBase
    {
        internal MonthTextProcessor(DateTime date, IEnumerable<FileEntry> entries, string userName) : base(date, entries, userName)
        {
        }

        internal override string GetText()
        {
            var totalLength = this.Entries.Select(this.GetVideoLength).Sum();

            var dailyLength = this.GetDailyLength(totalLength);

            var text = new StringBuilder();

            text.AppendLine(this.Date.ToString("MMMM yyyy"));

            text.Append("Total: ");
            text.AppendLine(TimeHelper.FormatTime(totalLength));

            text.Append("Average: ");
            text.Append(TimeHelper.FormatTime(dailyLength));
            text.AppendLine(" per day");

            return text.ToString();
        }

        protected override bool WatchContainsDate(Watch watch) => watch.MatchesMonth(this.Date);

        private uint GetDailyLength(uint totalLength)
        {
            var daysInMonth = DateTime.DaysInMonth(this.Date.Year, this.Date.Month);

            var lastDayInMonth = new DateTime(this.Date.Year, this.Date.Month, daysInMonth);

            var today = DateTime.Now.Date;

            var days = today > lastDayInMonth ? lastDayInMonth.Day : today.Day;

            var dailyLength = (uint)(totalLength / days);

            return dailyLength;
        }
    }
}