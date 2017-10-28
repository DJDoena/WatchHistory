using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DoenaSoft.AbstractionLayer.IOServices;
using DoenaSoft.WatchHistory.Data;
using DoenaSoft.WatchHistory.Data.Implementations;

namespace DoenaSoft.WatchHistory.Implementations
{
    internal static class ViewModelHelper
    {
        internal static ObservableCollection<IFileEntryViewModel> GetSortedEntries(IEnumerable<FileEntry> modelEntries
            , String userName
            , IDataManager dataManager
            , IIOServices ioServices)
        {
            List<FileEntryViewModel> viewModelEntries = modelEntries.Select(item => new FileEntryViewModel(item, userName, dataManager, ioServices)).ToList();

            viewModelEntries.Sort(Compare);

            return (new ObservableCollection<IFileEntryViewModel>(viewModelEntries));
        }

        private static Int32 Compare(FileEntryViewModel left
            , FileEntryViewModel right)
        {
            String leftName = PadName(left.Name);

            String rightName = PadName(right.Name);

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

        private static void TryPadName(ref String part)
        {
            UInt32 number;
            if (UInt32.TryParse(part, out number))
            {
                part = number.ToString("D10");
            }
        }
    }
}