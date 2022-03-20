namespace DoenaSoft.WatchHistory.Data
{
    using System;
    using System.Diagnostics;
    using System.Xml;
    using System.Xml.Serialization;
    using MediaInfoHelper;

    [XmlRoot]
    public sealed class Files
    {
        [XmlArray("Entries")]
        [XmlArrayItem("Entry")]
        public FileEntry[] Entries;

        [XmlAttribute]
        public decimal Version;

        [XmlAnyAttribute]
        public XmlAttribute[] AnyAttributes;

        [XmlAnyElement]
        public XmlElement[] AnyElements;

        public Files()
        {
            Version = 2.4m;
        }
    }

    [DebuggerDisplay("File: {FullName}")]
    public sealed partial class FileEntry
    {
        private User[] _users;

        private uint _videoLength;

        [XmlElement]
        public string FullName;

        [XmlAttribute]
        public DateTime CreationTime
        {
            get => CreationTimeValue ?? default(DateTime).ToUniversalTime();
            set
            {
                var newValue = value.Conform();

                if (CreationTimeValue.HasValue && CreationTimeValue.Value != default && CreationTimeValue.Value < newValue)
                {
                    //System.Diagnostics.Debugger.Launch();
                }

                CreationTimeValue = newValue;
            }
        }

        [XmlArray("Users")]
        [XmlArrayItem("User")]
        public User[] Users
        {
            get => _users;
            set
            {
                _users = value;

                UsersChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [XmlElement]
        public string Note;

        [XmlAttribute]
        public uint VideoLength
        {
            get => _videoLength;
            set
            {
                if (_videoLength != value)
                {
                    _videoLength = value;

                    VideoLengthChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        [XmlIgnore]
        public bool VideoLengthSpecified => this.VideoLength > 0;

        [XmlAttribute]
        public string Title;

        [XmlAttribute]
        public bool FileExists = true;

        [XmlIgnore]
        public bool TitleSpecified => !string.IsNullOrWhiteSpace(Title);

        [XmlAnyAttribute]
        public XmlAttribute[] AnyAttributes;

        [XmlAnyElement]
        public XmlElement[] AnyElements;

        [XmlIgnore]
        internal DateTime? CreationTimeValue;

        [XmlIgnore]
        public string Key => GetKey(FullName);

        public static string GetKey(string fullName)
        {
            string key;
            if (string.IsNullOrEmpty(fullName))
            {
                key = string.Empty;
            }
            else
            {
                var parts = fullName.Split('\\');

                var end = fullName.EndsWith(Constants.YoutubeFileExtension)
                    ? parts.Length - 1
                    : parts.Length;

                for (int partIndex = 0; partIndex < end; partIndex++)
                {
                    parts[partIndex] = parts[partIndex].ToLowerInvariant().TrimEnd('.');
                }

                key = string.Join("\\", parts);
            }

            return key;
        }

        public override int GetHashCode() => this.Key.GetHashCode();

        public override bool Equals(object obj)
        {
            if (!(obj is FileEntry other))
            {
                return false;
            }

            bool equals = this.Key == other.Key;

            return equals;
        }

        public event EventHandler UsersChanged;

        public event EventHandler VideoLengthChanged;
    }

    [DebuggerDisplay("User: {UserName}")]
    public sealed class User : IEquatable<User>
    {
        private Watch[] _watches;

        [XmlAttribute]
        public string UserName;

        [XmlAttribute]
        public bool Ignore;

        [XmlIgnore]
        public bool IgnoreSpecified => Ignore;

        [XmlArray("Watches")]
        [XmlArrayItem("Watched")]
        public Watch[] Watches
        {
            get
            {
                if ((_watches == null) || (_watches.Length == 0))
                {
                    return null;
                }

                return _watches;
            }
            set
            {
                _watches = value;

                WatchesChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [XmlAnyAttribute]
        public XmlAttribute[] AnyAttributes;

        [XmlAnyElement]
        public XmlElement[] AnyElements;

        public event EventHandler WatchesChanged;

        public override int GetHashCode() => UserName?.GetHashCode() ?? 0;

        public override bool Equals(object obj) => this.Equals(obj as User);

        public bool Equals(User other) => other != null && UserName == other.UserName;
    }

    [DebuggerDisplay("Watched: {Value}")]
    public sealed class Watch : IEquatable<Watch>
    {
        private DateTime _watched;

        [XmlAttribute]
        public DateTime Value
        {
            get => _watched;
            set => _watched = value.Conform();
        }

        [XmlAttribute]
        public string Source;

        [XmlIgnore]
        public bool SourceSpecified => !string.IsNullOrWhiteSpace(Source);

        [XmlAnyAttribute]
        public XmlAttribute[] AnyAttributes;

        [XmlAnyElement]
        public XmlElement[] AnyElements;

        public override int GetHashCode() => this.Value.GetHashCode();

        public override bool Equals(object obj) => (this.Equals(obj as Watch));

        public bool Equals(Watch other) => other != null && this.Value == other.Value && Source == other.Source;
    }
}