namespace DoenaSoft.WatchHistory
{
    using System;
    using System.Collections.Generic;
    using System.IO;
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

            CreateBackup();

            if (files?.Entries?.Length > 0)
            {
                files.Entries = Clean(files.Entries);
            }

            Serializer<Files>.Serialize(Environment.DataFile, files);
        }

        private static void CreateBackup()
        {
            String file = Environment.DataFile;

            Int32 lastIndexOf = file.LastIndexOf(".");

            String extension = file.Substring(lastIndexOf);

            String fileBaseName = file.Substring(0, lastIndexOf);

            try
            {
                const Int32 MaximumBackups = 9;

                String fileName = fileBaseName + "." + MaximumBackups.ToString() + extension;

                if (IOServices.File.Exists(fileName))
                {
                    IOServices.File.Delete(fileName);
                }

                for (Int32 i = MaximumBackups - 1; i > 0; i--)
                {
                    String fileName2 = fileBaseName + "." + i.ToString() + extension;

                    if (IOServices.File.Exists(fileName2))
                    {
                        IOServices.File.Move(fileName2, fileName);
                    }

                    fileName = fileName2;
                }

                if (IOServices.File.Exists(file))
                {
                    IOServices.File.Copy(file, fileName);
                }
            }
            catch (IOException)
            { }
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