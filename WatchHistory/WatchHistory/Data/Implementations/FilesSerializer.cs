namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System.Text;
    using AbstractionLayer.IOServices;
    using WatchHistory.Implementations;

    internal sealed class FilesSerializer : IFilesSerializer
    {
        private readonly IIOServices _ioServices;

        public FilesSerializer(IIOServices ioServices)
        {
            _ioServices = ioServices;
        }

        #region IFilesSerializer

        public Files LoadData(string fileName)
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

        public void SaveFile(string fileName
            , Files files)
            => SerializerHelper.Serialize(_ioServices, fileName, files);

        public void CreateBackup(string fileName)
        {
            int lastIndexOf = fileName.LastIndexOf(".");

            string extension = fileName.Substring(lastIndexOf);

            string fileBaseName = fileName.Substring(0, lastIndexOf);

            try
            {
                const int MaximumBackups = 9;

                string newFileName = fileBaseName + "." + MaximumBackups.ToString() + extension;

                if (_ioServices.File.Exists(newFileName))
                {
                    _ioServices.File.Delete(newFileName);
                }

                for (int backupIndex = MaximumBackups - 1; backupIndex > 0; backupIndex--)
                {
                    string oldFileName = fileBaseName + "." + backupIndex.ToString() + extension;

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

        private Files ReadXml(string fileName)
            => (IsVersion2File(fileName)) ? ReadVersion2File(fileName) : ReadVersion1File(fileName);

        private bool IsVersion2File(string fileName)
        {
            using (System.IO.Stream fs = _ioServices.GetFileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(fs, Encoding.UTF8))
                {
                    int lineNumber = 1;

                    string line = string.Empty;

                    while ((sr.EndOfStream == false) && (lineNumber < 3))
                    {
                        line = sr.ReadLine();

                        lineNumber++;
                    }

                    return (line.Contains("Version=\"2."));
                }
            }
        }

        private Files ReadVersion2File(string fileName)
            => SerializerHelper.Deserialize<Files>(_ioServices, fileName);

        private Files ReadVersion1File(string fileName)
        {
            v1_0.Files oldFiles = SerializerHelper.Deserialize<v1_0.Files>(_ioServices, fileName);

            Files newFiles = FilesModelConverter.Convert(oldFiles);

            return (newFiles);
        }
    }
}