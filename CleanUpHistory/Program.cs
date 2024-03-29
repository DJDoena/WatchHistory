﻿using System;
using System.Collections.Generic;
using System.IO;
using DoenaSoft.AbstractionLayer.IOServices;
using DoenaSoft.WatchHistory.Data;
using DoenaSoft.WatchHistory.Implementations;

namespace DoenaSoft.WatchHistory
{
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
            var files = SerializerHelper.Deserialize<Files>(IOServices, Environment.DataFile);

            CreateBackup();

            if (files?.Entries?.Length > 0)
            {
                files.Entries = Clean(files.Entries);
            }

            SerializerHelper.Serialize(IOServices, Environment.DataFile, files);
        }

        private static void CreateBackup()
        {
            var file = Environment.DataFile;

            var lastIndexOf = file.LastIndexOf(".");

            var extension = file.Substring(lastIndexOf);

            var fileBaseName = file.Substring(0, lastIndexOf);

            try
            {
                const Int32 MaximumBackups = 9;

                var fileName = fileBaseName + "." + MaximumBackups.ToString() + extension;

                if (IOServices.File.Exists(fileName))
                {
                    IOServices.File.Delete(fileName);
                }

                for (var i = MaximumBackups - 1; i > 0; i--)
                {
                    var fileName2 = fileBaseName + "." + i.ToString() + extension;

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
            var output = new List<FileEntry>(input);

            for (var index = output.Count - 1; index >= 0; index--)
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