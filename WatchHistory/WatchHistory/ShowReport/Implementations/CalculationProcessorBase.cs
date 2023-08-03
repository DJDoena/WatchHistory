using System;
using System.Collections.Generic;
using System.Linq;
using DoenaSoft.MediaInfoHelper.DataObjects;
using DoenaSoft.MediaInfoHelper.Readers;
using DoenaSoft.WatchHistory.Data;

namespace DoenaSoft.WatchHistory.ShowReport.Implementations
{
    internal abstract class CalculationProcessorBase
    {
        protected readonly IDataManager _dataManager;

        protected readonly string _userName;

        protected readonly DateTime _date;

        protected CalculationProcessorBase(IDataManager dataManager, string userName, DateTime date)
        {
            _dataManager = dataManager;
            _userName = userName;
            _date = date.Date;
        }

        internal abstract IEnumerable<FileEntry> GetEntries();

        protected abstract bool WatchContainsDate(Watch watch);

        protected List<FileEntry> GetFilteredEntries()
        {
            var entries = _dataManager.GetFiles().Where(ContainsUserWithWatchedDate).ToList();

            foreach (var entry in entries)
            {
                var mediaFileData = new MediaFile(entry.FullName, entry.CreationTime, entry.VideoLength);

                (new VideoReader(mediaFileData)).DetermineLength();

                if (mediaFileData.HasChanged)
                {
                    entry.VideoLength = mediaFileData.Length;
                    entry.CreationTime = mediaFileData.CreationTime;
                }
            }

            return entries;
        }

        private bool ContainsUserWithWatchedDate(FileEntry entry) => entry.GetWatchesByUserAndWatchDate(_userName, WatchContainsDate).Any();
    }
}