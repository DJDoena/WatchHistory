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
        internal static ObservableCollection<IFileEntryViewModel> GetSortedEntries(IEnumerable<FileEntry> modelEntries, string userName, IDataManager dataManager, IIOServices ioServices, SortColumn sortColumn, bool ascending)
        {
            var viewModelEntries = modelEntries.Select(item => new FileEntryViewModel(item, userName, dataManager, ioServices)).ToList();

            viewModelEntries.Sort((left, right) => Compare(left, right, sortColumn, ascending, userName, dataManager));

            return new ObservableCollection<IFileEntryViewModel>(viewModelEntries);
        }

        internal static string GetFormattedDateTime(DateTime dateTime) => $"{dateTime.ToShortDateString()} {dateTime.ToShortTimeString()}";

        private static int Compare(FileEntryViewModel left, FileEntryViewModel right, SortColumn sortColumn, bool ascending, string userName, IDataManager dataManager)
        {
            var compare = 0;

            switch (sortColumn)
            {
                case SortColumn.LastWatched:
                    {
                        compare = ascending
                            ? CompareLastWatched(left, right, userName, dataManager)
                            : CompareLastWatched(right, left, userName, dataManager);

                        break;
                    }
                case SortColumn.CreationTime:
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

            return compare;
        }

        private static int CompareLastWatched(FileEntryViewModel left, FileEntryViewModel right, string userName, IDataManager dataManager)
        {
            var leftLastWatched = dataManager.GetLastWatched(left.Entry, userName);

            var rightLastWatched = dataManager.GetLastWatched(right.Entry, userName);

            return leftLastWatched.CompareTo(rightLastWatched);
        }

        private static int CompareCreationTime(FileEntryViewModel left, FileEntryViewModel right, IDataManager dataManager)
        {
            var leftCreationTime = left.Entry.GetCreationTime(dataManager);

            var rightCreationTime = right.Entry.GetCreationTime(dataManager);

            return leftCreationTime.CompareTo(rightCreationTime);
        }

        private static int CompareName(FileEntryViewModel left, FileEntryViewModel right)
        {
            var leftName = PadName(left.Name);

            ReplaceArticles(ref leftName);

            var rightName = PadName(right.Name);

            ReplaceArticles(ref rightName);

            return leftName.CompareTo(rightName);
        }

        private static string PadName(string name)
        {
            var parts = name.Split(' ', '\\', '.', ',');

            for (var i = 0; i < parts.Length; i++)
            {
                TryPadName(ref parts[i]);
            }

            name = string.Join(" ", parts);

            return name;
        }

        internal static string GetFormattedRunningTime(uint runningTime)
        {
            var hours = runningTime / 3600;

            var modulo = runningTime % 3600;

            var minutes = modulo / 60;

            var seconds = modulo % 60;

            var text = $"{minutes:D2}:{seconds:D2}";

            if (hours > 0)
            {
                text = $"{hours:D2}:{text}";
            }

            return text;
        }

        private static void TryPadName(ref string part)
        {
            if (uint.TryParse(part, out var number))
            {
                part = number.ToString("D10");
            }
        }

        private static void ReplaceArticles(ref string name)
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