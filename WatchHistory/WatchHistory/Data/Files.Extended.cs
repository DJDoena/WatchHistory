namespace DoenaSoft.WatchHistory.Data
{
    using System;
    using System.Xml.Serialization;

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
            else if (fullName.EndsWith(Constants.YoutubeFileExtension))
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
            if ((m_CreationTime.HasValue == false) || (m_CreationTime.Value.Ticks == 0))
            {
                m_CreationTime = dataManager.GetCreationTime(this).Conform();
            }

            return m_CreationTime.Value.ToLocalTime();
        }

        internal uint GetVideoLength(IDataManager dataManager)
        {
            if (VideoLengthSpecified == false)
            {
                VideoLength = dataManager.GetVideoLength(this);
            }

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