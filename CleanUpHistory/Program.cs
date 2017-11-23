namespace DoenaSoft.WatchHistory
{
    using System;
    using System.Collections.Generic;
    using AbstractionLayer.IOServices;
    using AbstractionLayer.IOServices.Implementations;
    using Data;
    using ToolBox.Generics;

    public static class Program
    {
        private static IIOServices IOServices { get; set; }

        public static void Main()
        {
            IOServices = new IOServices();

            Environment.Init(IOServices);

            try
            {
                TryClean();
            }
            catch
            { }
        }

        private static void TryClean()
        {
            Files files = Serializer<Files>.Deserialize(Environment.DataFile);

            if (files?.Entries?.Length > 0)
            {
                files.Entries = Clean(files.Entries);
            }

            Serializer<Files>.Serialize(Environment.DataFile, files);
        }

        private static FileEntry[] Clean(FileEntry[] input)
        {
            List<FileEntry> output = new List<FileEntry>(input);

            for (Int32 index = output.Count - 1; index >= 0; index--)
            {
                TryClean(output, index);
            }

            return (output.ToArray());
        }

        private static void TryClean(List<FileEntry> entries
            , Int32 index)
        {
            if (IOServices.File.Exists(entries[index].FullName) == false)
            {
                entries.RemoveAt(index);
            }
        }
    }
}