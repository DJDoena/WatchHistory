namespace DoenaSoft.WatchHistory.Data
{
    using System;
    using System.Xml.Serialization;
    using MediaInfoHelper;

    public partial class FileEntry
    {
        [XmlIgnore]
        public string Key
            => GetKey(FullName);

        public static string GetKey(string fullName)
        {
            string key;
            if (string.IsNullOrEmpty(fullName))
            {
                key = string.Empty;
            }
            else if (fullName.EndsWith(MediaInfoHelper.Constants.YoutubeFileExtension))
            {
                var parts = fullName.Split('\\');

                for (int partIndex = 0; partIndex < parts.Length - 1; partIndex++)
                {
                    parts[partIndex] = parts[partIndex].ToLowerInvariant();
                }

                key = string.Join("\\", parts);
            }
            else
            {
                key = fullName.ToLowerInvariant();
            }

            return key;
        }

        internal DateTime GetCreationTime(IDataManager dataManager)
        {
            if ((_creationTime.HasValue == false) || (_creationTime.Value.Ticks == 0))
            {
                _creationTime = dataManager.GetCreationTime(this).Conform();
            }

            return _creationTime.Value.ToLocalTime();
        }

        internal uint GetVideoLength(IDataManager dataManager)
        {
            dataManager.DetermineVideoLength(this);

            return VideoLength;
        }

        public override int GetHashCode() => Key.GetHashCode();

        public override bool Equals(object obj)
        {
            if (!(obj is FileEntry other))
            {
                return false;
            }

            bool equals = Key == other.Key;

            return equals;
        }
    }
}