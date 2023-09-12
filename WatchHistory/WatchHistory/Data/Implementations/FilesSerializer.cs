namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AbstractionLayer.IOServices;
    using WatchHistory.Implementations;

    internal sealed class FilesSerializer : IFilesSerializer
    {
        private const int MaximumBackups = 9;

        private readonly IIOServices _ioServices;

        public FilesSerializer(IIOServices ioServices)
        {
            _ioServices = ioServices;
        }

        #region IFilesSerializer

        public IEnumerable<FileEntry> LoadData(string fileName)
        {
            IEnumerable<FileEntry> entries;
            try
            {
                entries = this.ReadXml(fileName);
            }
            catch
            {
                entries = this.TryRestoreDataBackup(fileName);
            }

            return entries ?? Enumerable.Empty<FileEntry>();
        }

        private IEnumerable<FileEntry> TryRestoreDataBackup(string fileName)
        {
            IEnumerable<FileEntry> entries = null;

            var lastIndexOf = fileName.LastIndexOf(".");

            var extension = fileName.Substring(lastIndexOf);

            var fileBaseName = fileName.Substring(0, lastIndexOf);

            for (var backupIndex = 1; backupIndex <= MaximumBackups; backupIndex++)
            {
                var newFileName = fileBaseName + "." + backupIndex.ToString() + extension;

                if (_ioServices.File.Exists(newFileName))
                {
                    try
                    {
                        entries = this.ReadXml(newFileName);

                        break;
                    }
                    catch
                    { }
                }
            }

            return entries;
        }

        public DefaultValues LoadSettings(string fileName)
        {
            DefaultValues defaultValues;
            try
            {
                var settings = SerializerHelper.Deserialize<Settings>(_ioServices, fileName);

                defaultValues = settings.DefaultValues;
            }
            catch
            {
                defaultValues = this.TryRestoreSettingsBackup(fileName);
            }

            if (defaultValues == null)
            {
                defaultValues = new DefaultValues();
            }

            if (defaultValues.Users == null)
            {
                defaultValues.Users = new string[0];
            }

            if (defaultValues.RootFolders == null)
            {
                defaultValues.RootFolders = new string[0];
            }

            if (defaultValues.FileExtensions == null)
            {
                defaultValues.FileExtensions = new string[0];
            }

            return defaultValues;
        }

        private DefaultValues TryRestoreSettingsBackup(string fileName)
        {
            DefaultValues defaultValues = null;

            var lastIndexOf = fileName.LastIndexOf(".");

            var extension = fileName.Substring(lastIndexOf);

            var fileBaseName = fileName.Substring(0, lastIndexOf);

            for (var backupIndex = 1; backupIndex <= MaximumBackups; backupIndex++)
            {
                var newFileName = fileBaseName + "." + backupIndex.ToString() + extension;

                if (_ioServices.File.Exists(newFileName))
                {
                    try
                    {
                        var settings = SerializerHelper.Deserialize<Settings>(_ioServices, newFileName);

                        defaultValues = settings.DefaultValues;

                        break;
                    }
                    catch
                    { }
                }
            }

            return defaultValues;
        }

        public void SaveData(string fileName, IEnumerable<FileEntry> entries)
        {
            var files = new Files()
            {
                Entries = entries.ToArray(),
            };

            SerializerHelper.Serialize(_ioServices, fileName, files);
        }

        public void SaveSettings(string fileName, DefaultValues defaultValues)
        {
            var settings = new Settings()
            {
                DefaultValues = defaultValues,
            };

            SerializerHelper.Serialize(_ioServices, fileName, settings);
        }

        public void CreateBackup(string fileName)
        {
            var lastIndexOf = fileName.LastIndexOf(".");

            var extension = fileName.Substring(lastIndexOf);

            var fileBaseName = fileName.Substring(0, lastIndexOf);

            try
            {
                var newFileName = fileBaseName + "." + MaximumBackups.ToString() + extension;

                if (_ioServices.File.Exists(newFileName))
                {
                    _ioServices.File.Delete(newFileName);
                }

                for (var backupIndex = MaximumBackups - 1; backupIndex > 0; backupIndex--)
                {
                    var oldFileName = fileBaseName + "." + backupIndex.ToString() + extension;

                    if (_ioServices.File.Exists(oldFileName))
                    {
                        _ioServices.File.Move(oldFileName, newFileName);
                    }

                    newFileName = oldFileName;
                }

                if (_ioServices.File.Exists(fileName))
                {
                    _ioServices.File.Copy(fileName, newFileName);
                }
            }
            catch (System.IO.IOException)
            { }
        }



        #endregion

        private IEnumerable<FileEntry> ReadXml(string fileName) => (this.IsVersion2File(fileName)) ? this.ReadVersion2File(fileName) : this.ReadVersion1File(fileName);

        private bool IsVersion2File(string fileName)
        {
            using (var fs = _ioServices.GetFileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
            {
                using (var sr = new System.IO.StreamReader(fs, Encoding.UTF8))
                {
                    var lineNumber = 1;

                    var line = string.Empty;

                    while ((sr.EndOfStream == false) && (lineNumber < 3))
                    {
                        line = sr.ReadLine();

                        lineNumber++;
                    }

                    return line.Contains("Version=\"2.");
                }
            }
        }

        private IEnumerable<FileEntry> ReadVersion2File(string fileName) => SerializerHelper.Deserialize<Files>(_ioServices, fileName).Entries;

        private IEnumerable<FileEntry> ReadVersion1File(string fileName)
        {
            var oldFiles = SerializerHelper.Deserialize<v1_0.Files>(_ioServices, fileName);

            var newFiles = FilesModelConverter.Convert(oldFiles);

            return newFiles.Entries;
        }
    }
}