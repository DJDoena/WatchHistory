namespace DoenaSoft.WatchHistory.Data.v2_1
{
    using System;
    using System.Diagnostics;
    using System.Xml;
    using System.Xml.Serialization;
    using MediaInfoHelper;
    using ToolBox.Extensions;

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
            Version = 2.1m;
        }
    }

    [DebuggerDisplay("File: {FullName}")]
    public sealed partial class FileEntry
    {
        private User[] _users;

        private DateTime? _creationTime;

        [XmlElement]
        public string FullName;

        [XmlIgnore]
        public string Key
            => FullName?.ToLower() ?? string.Empty;

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
        public uint VideoLength;

        [XmlIgnore]
        public bool VideoLengthSpecified;

        [XmlAnyAttribute]
        public XmlAttribute[] AnyAttributes;

        [XmlAnyElement]
        public XmlElement[] AnyElements;

        public event EventHandler UsersChanged;
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
        public bool IgnoreSpecified
            => Ignore;

        [XmlArray("Watches")]
        [XmlArrayItem("Watched")]
        public Watch[] Watches
        {
            get
            {
                if ((_watches == null) || (_watches.Length == 0))
                {
                    return (null);
                }

                return (_watches);
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

        public override int GetHashCode()
            => UserName?.GetHashCode() ?? 0;

        public override bool Equals(object obj)
            => Equals(obj as User);

        public bool Equals(User other)
        {
            if (other == null)
            {
                return (false);
            }

            return (UserName == other.UserName);
        }
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
        public bool SourceSpecified
            => Source.IsNotEmpty();

        [XmlAnyAttribute]
        public XmlAttribute[] AnyAttributes;

        [XmlAnyElement]
        public XmlElement[] AnyElements;

        public override int GetHashCode()
            => Value.GetHashCode();

        public override bool Equals(object obj)
            => (Equals(obj as Watch));

        public bool Equals(Watch other)
        {
            if (other == null)
            {
                return (false);
            }

            return ((Value == other.Value) && (Source == other.Source));
        }
    }
}