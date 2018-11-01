namespace DoenaSoft.WatchHistory.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using AbstractionLayer.IOServices;
    using Data;
    using Data.Implementations;

    internal static class ViewModelHelper
    {
        internal static ObservableCollection<IFileEntryViewModel> GetSortedEntries(IEnumerable<FileEntry> modelEntries
            , String userName
            , IDataManager dataManager
            , IIOServices ioServices
            , SortColumn sortColumn
            , Boolean ascending)
        {
            List<FileEntryViewModel> viewModelEntries = modelEntries.Select(item => new FileEntryViewModel(item, userName, dataManager, ioServices)).ToList();

            viewModelEntries.Sort((left, right) => Compare(left, right, sortColumn, ascending, userName, dataManager));

            return (new ObservableCollection<IFileEntryViewModel>(viewModelEntries));
        }

        internal static String GetFormattedDateTime(DateTime dateTime)
            => $"{dateTime.ToShortDateString()} {dateTime.ToShortTimeString()}";

        private static Int32 Compare(FileEntryViewModel left
            , FileEntryViewModel right
            , SortColumn sortColumn
            , Boolean ascending
            , String userName
            , IDataManager dataManager)
        {
            Int32 compare = 0;

            switch (sortColumn)
            {
                case (SortColumn.LastWatched):
                    {
                        compare = ascending
                            ? CompareLastWatched(left, right, userName, dataManager)
                            : CompareLastWatched(right, left, userName, dataManager);

                        break;
                    }
                case (SortColumn.CreationTime):
                    {
                        compare = ascending
                            ? CompareCreationTime(left, right, dataManager)
                            : CompareCreationTime(right, left, dataManager);

                        break;
                    }
            }

            if (compare == 0)
            {
                compare = ascending ? CompareName(left, right) : CompareName(right, left);
            }

            return (compare);
        }

        private static Int32 CompareLastWatched(FileEntryViewModel left
            , FileEntryViewModel right
            , String userName
            , IDataManager dataManager)
        {
            DateTime leftLastWatched = dataManager.GetLastWatched(left.FileEntry, userName);

            DateTime rightLastWatched = dataManager.GetLastWatched(right.FileEntry, userName);

            return (leftLastWatched.CompareTo(rightLastWatched));
        }

        private static Int32 CompareCreationTime(FileEntryViewModel left
            , FileEntryViewModel right
            , IDataManager dataManager)
        {
            DateTime leftCreationTime = left.FileEntry.GetCreationTime(dataManager);

            DateTime rightCreationTime = right.FileEntry.GetCreationTime(dataManager);

            return (leftCreationTime.CompareTo(rightCreationTime));
        }

        private static Int32 CompareName(FileEntryViewModel left
            , FileEntryViewModel right)
        {
            String leftName = PadName(left.Name);

            ReplaceArticles(ref leftName);

            String rightName = PadName(right.Name);

            ReplaceArticles(ref rightName);

            return (leftName.CompareTo(rightName));
        }

        private static String PadName(String name)
        {
            String[] parts = name.Split(' ', '\\', '.', ',');

            for (Int32 i = 0; i < parts.Length; i++)
            {
                TryPadName(ref parts[i]);
            }

            name = String.Join(" ", parts);

            return (name);
        }

        internal static string GetFormattedRunningTime(UInt32 runningTime)
        {
            UInt32 hours = runningTime / 3600;

            UInt32 modulo = runningTime % 3600;

            UInt32 minutes = modulo / 60;

            UInt32 seconds = modulo % 60;

            String text = $"{minutes:D2}:{seconds:D2}";

            if (hours > 0)
            {
                text = $"{hours:D2}:{text}";
            }

            return text;
        }

        private static void TryPadName(ref String part)
        {
            if (UInt32.TryParse(part, out uint number))
            {
                part = number.ToString("D10");
            }
        }

        private static void ReplaceArticles(ref String name)
        {
            if (name.StartsWith("the ", StringComparison.InvariantCultureIgnoreCase))
            {
                name = name.Substring(4);
            }
            else if (name.StartsWith("a ", StringComparison.InvariantCultureIgnoreCase))
            {
                name = name.Substring(2);
            }
            else if (name.StartsWith("an ", StringComparison.InvariantCultureIgnoreCase))
            {
                name = name.Substring(3);
            }
            else if (name.StartsWith("der ", StringComparison.InvariantCultureIgnoreCase))
            {
                name = name.Substring(4);
            }
            else if (name.StartsWith("das ", StringComparison.InvariantCultureIgnoreCase))
            {
                name = name.Substring(4);
            }
            else if (name.StartsWith("ein ", StringComparison.InvariantCultureIgnoreCase))
            {
                name = name.Substring(4);
            }
            else if (name.StartsWith("eine ", StringComparison.InvariantCultureIgnoreCase))
            {
                name = name.Substring(5);
            }
        }
    }
}