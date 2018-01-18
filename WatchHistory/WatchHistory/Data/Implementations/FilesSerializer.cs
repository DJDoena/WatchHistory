namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System;
    using System.IO;
    using System.Text;
    using AbstractionLayer.IOServices;
    using DVDProfiler.DVDProfilerHelper;

    internal sealed class FilesSerializer : IFilesSerializer
    {
        private readonly IIOServices IOServices;

        public FilesSerializer(IIOServices ioServices)
        {
            IOServices = ioServices;
        }

        #region IFilesSerializer
        
        public Files LoadData(String file)
        {
            Files files;
            try
            {
                files = ReadXml(file);
            }
            catch
            {
                files = new Files();
            }

            return (files);
        }

        public void SaveFile(String file
            , Files files)
        {
            Serializer<Files>.Serialize(file, files);
        }

        public void CreateBackup(String file)
        {
            Int32 lastIndexOf = file.LastIndexOf(".");

            String extension = file.Substring(lastIndexOf);

            String fileBaseName = file.Substring(0, lastIndexOf);

            try
            {
                String fileName = fileBaseName + ".5" + extension;

                if (IOServices.File.Exists(fileName))
                {
                    IOServices.File.Delete(fileName);
                }

                for (Int32 i = 4; i > 0; i--)
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

        #endregion

        private Files ReadXml(String file)
            => ((IsVersion2File(file)) ? ReadVersion2File(file) : ReadVersion1File(file));

        private Boolean IsVersion2File(String file)
        {
            using (Stream fs = IOServices.GetFileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
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

        private Files ReadVersion2File(String file)
        {
            using (Stream fs = IOServices.GetFileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Files files = Serializer<Files>.Deserialize(fs);

                return (files);
            }
        }

        private Files ReadVersion1File(String file)
        {
            using (Stream fs = IOServices.GetFileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                v1_0.Files oldFiles = Serializer<v1_0.Files>.Deserialize(fs);

                Files newFiles = FilesModelConverter.Convert(oldFiles);

                return (newFiles);
            }
        }
    }
}