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
            Version = 2.2m;
        }
    }

    [DebuggerDisplay("File: {FullName}")]
    public sealed partial class FileEntry
    {
        private User[] _users;

        private DateTime? _creationTime;

        private uint _videoLength;

        [XmlElement]
        public string FullName;

        [XmlAttribute]
        public DateTime CreationTime
        {
            get => _creationTime ?? new DateTime(0, DateTimeKind.Utc);
            set => _creationTime = value.Conform();
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
        public bool VideoLengthSpecified => VideoLength > 0;

        [XmlAttribute]
        public string Title;

        [XmlIgnore]
        public bool TitleSpecified => !string.IsNullOrWhiteSpace(Title);

        [XmlAnyAttribute]
        public XmlAttribute[] AnyAttributes;

        [XmlAnyElement]
        public XmlElement[] AnyElements;

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

        public override bool Equals(object obj) => Equals(obj as User);

        public bool Equals(User other) => other != null ? UserName == other.UserName : false;
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

        public override int GetHashCode() => Value.GetHashCode();

        public override bool Equals(object obj) => (Equals(obj as Watch));

        public bool Equals(Watch other) => other != null ? Value == other.Value && Source == other.Source : false;
    }
}