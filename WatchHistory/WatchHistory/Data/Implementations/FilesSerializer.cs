﻿namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System;
    using System.IO;
    using System.Text;
    using AbstractionLayer.IOServices;
    using WatchHistory.Implementations;

    internal sealed class FilesSerializer : IFilesSerializer
    {
        private readonly IIOServices _IOServices;

        public FilesSerializer(IIOServices ioServices)
        {
            _IOServices = ioServices;
        }

        #region IFilesSerializer

        public Files LoadData(String fileName)
        {
            Files files;
            try
            {
                files = ReadXml(fileName);
            }
            catch
            {
                files = new Files();
            }

            return (files);
        }

        public void SaveFile(String fileName
            , Files files)
            => SerializerHelper.Serialize(_IOServices, fileName, files);

        public void CreateBackup(String fileName)
        {
            Int32 lastIndexOf = fileName.LastIndexOf(".");

            String extension = fileName.Substring(lastIndexOf);

            String fileBaseName = fileName.Substring(0, lastIndexOf);

            try
            {
                const Int32 MaximumBackups = 9;

                String newFileName = fileBaseName + "." + MaximumBackups.ToString() + extension;

                if (_IOServices.File.Exists(newFileName))
                {
                    _IOServices.File.Delete(newFileName);
                }

                for (Int32 backupIndex = MaximumBackups - 1; backupIndex > 0; backupIndex--)
                {
                    String oldFileName = fileBaseName + "." + backupIndex.ToString() + extension;

                    if (_IOServices.File.Exists(oldFileName))
                    {
                        _IOServices.File.Move(oldFileName, newFileName);
                    }

                    newFileName = oldFileName;
                }

                if (_IOServices.File.Exists(fileName))
                {
                    _IOServices.File.Copy(fileName, newFileName);
                }
            }
            catch (IOException)
            { }
        }

        #endregion

        private Files ReadXml(String fileName)
            => (IsVersion2File(fileName)) ? ReadVersion2File(fileName) : ReadVersion1File(fileName);

        private Boolean IsVersion2File(String fileName)
        {
            using (Stream fs = _IOServices.GetFileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                {
                    Int32 lineNumber = 1;

                    String line = String.Empty;

                    while ((sr.EndOfStream == false) && (lineNumber < 3))
                    {
                        line = sr.ReadLine();

                        lineNumber++;
                    }

                    return (line.Contains("Version=\"2."));
                }
            }
        }

        private Files ReadVersion2File(String fileName)
            => SerializerHelper.Deserialize<Files>(_IOServices, fileName);

        private Files ReadVersion1File(String fileName)
        {
            v1_0.Files oldFiles = SerializerHelper.Deserialize<v1_0.Files>(_IOServices, fileName);

            Files newFiles = FilesModelConverter.Convert(oldFiles);

            return (newFiles);
        }
    }
}