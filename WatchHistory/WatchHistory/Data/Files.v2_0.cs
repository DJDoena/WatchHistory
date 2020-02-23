namespace DoenaSoft.WatchHistory.Data.v2_0
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
        public FileEntry[] Entries { get; set; }

        [XmlAttribute]
        public decimal Version { get; set; }

        [XmlAnyAttribute]
        public XmlAttribute[] AnyAttributes { get; set; }

        [XmlAnyElement]
        public XmlElement[] AnyElements { get; set; }

        public Files()
        {
            Version = 2.0m;
        }
    }

    [DebuggerDisplay("File: {FullName}")]
    public sealed partial class FileEntry
    {
        private User[] _users;

        private DateTime? _creationTime;

        [XmlElement]
        public string FullName { get; set; }

        [XmlAttribute]
        public DateTime CreationTime
        {
            get
            {
                return _creationTime ?? new DateTime(0, DateTimeKind.Utc);
            }
            set
            {
                _creationTime = value.Conform();
            }
        }

        [XmlArray("Users")]
        [XmlArrayItem("User")]
        public User[] Users
        {
            get
            {
                return _users;
            }
            set
            {
                _users = value;

                UsersChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [XmlAttribute]
        public int VideoLength { get; set; }

        [XmlIgnore]
        public bool VideoLengthSpecified { get; set; }

        [XmlAnyAttribute]
        public XmlAttribute[] AnyAttributes { get; set; }

        [XmlAnyElement]
        public XmlElement[] AnyElements { get; set; }

        public event EventHandler UsersChanged;
    }

    [DebuggerDisplay("User: {UserName}")]
    public sealed class User
    {
        private Watch[] _watches;

        [XmlAttribute]
        public string UserName { get; set; }

        [XmlAttribute]
        public bool Ignore { get; set; }

        [XmlIgnore]
        public bool IgnoreSpecified { get; set; }

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
        public XmlAttribute[] AnyAttributes { get; set; }

        [XmlAnyElement]
        public XmlElement[] AnyElements { get; set; }

        public event EventHandler WatchesChanged;
    }

    [DebuggerDisplay("Watched: {Value}")]
    public sealed class Watch
    {
        private DateTime _watched;

        [XmlAttribute]
        public DateTime Value
        {
            get
            {
                return _watched;
            }
            set
            {
                _watched = value.Conform();
            }
        }

        [XmlAnyAttribute]
        public XmlAttribute[] AnyAttributes { get; set; }

        [XmlAnyElement]
        public XmlElement[] AnyElements { get; set; }
    }
}