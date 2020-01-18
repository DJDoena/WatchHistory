namespace DoenaSoft.WatchHistory.Data.v1_0
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    [XmlRoot]
    public sealed class Files
    {
        [XmlArray("Entries")]
        [XmlArrayItem("Entry")]
        public FileEntry[] Entries { get; set; }

        [XmlAnyAttribute]
        public XmlAttribute[] AnyAttributes { get; set; }

        [XmlAnyElement]
        public XmlElement[] AnyElements { get; set; }
    }

    public sealed partial class FileEntry
    {
        private User[] _users;

        private DateTime? _creationTime;

        [XmlElement]
        public string FullName { get; set; }

        [XmlElement]
        public DateTime CreationTime
        {
            get
            {
                return ((_creationTime ?? new DateTime(0)).ToUniversalTime());
            }
            set
            {
                _creationTime = value;
            }
        }

        [XmlArray("Users")]
        [XmlArrayItem("User")]
        public User[] Users
        {
            get
            {
                return (_users);
            }
            set
            {
                _users = value;

                UsersChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [XmlAnyAttribute]
        public XmlAttribute[] AnyAttributes { get; set; }

        [XmlAnyElement]
        public XmlElement[] AnyElements { get; set; }

        public event EventHandler UsersChanged;
    }

    public sealed class User
    {
        private DateTime[] _watches;

        [XmlAttribute]
        public string UserName { get; set; }

        [XmlAttribute]
        public bool Ignore { get; set; }

        [XmlIgnore]
        public bool IgnoreSpecified { get; set; }

        [XmlArray("Watches")]
        [XmlArrayItem("Watched")]
        public DateTime[] Watches
        {
            get
            {
                if ((_watches == null) || (_watches.Length == 0))
                {
                    return (null);
                }

                for (int i = 0; i < _watches.Length; i++)
                {
                    _watches[i] = _watches[i].ToUniversalTime();
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
        public XmlAttribute[] AnyAttributes { get; set; }

        [XmlAnyElement]
        public XmlElement[] AnyElements { get; set; }

        public event EventHandler WatchesChanged;
    }
}