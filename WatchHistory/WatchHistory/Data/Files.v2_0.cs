namespace DoenaSoft.WatchHistory.Data.v2_0
{
    using System;
    using System.Diagnostics;
    using System.Xml;
    using System.Xml.Serialization;

    [XmlRoot]
    public sealed class Files
    {
        [XmlArray("Entries")]
        [XmlArrayItem("Entry")]
        public FileEntry[] Entries { get; set; }

        [XmlAttribute]
        public Decimal Version { get; set; }

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
        private User[] m_Users;

        private Nullable<DateTime> m_CreationTime;

        [XmlElement]
        public String FullName { get; set; }

        [XmlAttribute]
        public DateTime CreationTime
        {
            get
            {
                return (m_CreationTime ?? new DateTime(0, DateTimeKind.Utc));
            }
            set
            {
                m_CreationTime = value.Conform();
            }
        }

        [XmlArray("Users")]
        [XmlArrayItem("User")]
        public User[] Users
        {
            get
            {
                return (m_Users);
            }
            set
            {
                m_Users = value;

                UsersChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [XmlAttribute]
        public Int32 VideoLength { get; set; }

        [XmlIgnore]
        public Boolean VideoLengthSpecified { get; set; }

        [XmlAnyAttribute]
        public XmlAttribute[] AnyAttributes { get; set; }

        [XmlAnyElement]
        public XmlElement[] AnyElements { get; set; }

        public event EventHandler UsersChanged;
    }

    [DebuggerDisplay("User: {UserName}")]
    public sealed class User
    {
        private Watch[] m_Watches;

        [XmlAttribute]
        public String UserName { get; set; }

        [XmlAttribute]
        public Boolean Ignore { get; set; }

        [XmlIgnore]
        public Boolean IgnoreSpecified { get; set; }

        [XmlArray("Watches")]
        [XmlArrayItem("Watched")]
        public Watch[] Watches
        {
            get
            {
                if ((m_Watches == null) || (m_Watches.Length == 0))
                {
                    return (null);
                }

                return (m_Watches);
            }
            set
            {
                m_Watches = value;

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
        private DateTime m_Watched;

        [XmlAttribute]
        public DateTime Value
        {
            get
            {
                return m_Watched;
            }
            set
            {
                m_Watched = value.Conform();
            }
        }

        [XmlAnyAttribute]
        public XmlAttribute[] AnyAttributes { get; set; }

        [XmlAnyElement]
        public XmlElement[] AnyElements { get; set; }
    }
}