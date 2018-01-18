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
        private User[] m_Users;

        private Nullable<DateTime> m_CreationTime;

        [XmlElement]
        public String FullName { get; set; }

        [XmlElement]
        public DateTime CreationTime
        {
            get
            {
                return ((m_CreationTime ?? new DateTime(0)).ToUniversalTime());
            }
            set
            {
                m_CreationTime = value;
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

        [XmlAnyAttribute]
        public XmlAttribute[] AnyAttributes { get; set; }

        [XmlAnyElement]
        public XmlElement[] AnyElements { get; set; }

        public event EventHandler UsersChanged;
    }

    public sealed class User
    {
        private DateTime[] m_Watches;

        [XmlAttribute]
        public String UserName { get; set; }

        [XmlAttribute]
        public Boolean Ignore { get; set; }

        [XmlIgnore]
        public Boolean IgnoreSpecified { get; set; }

        [XmlArray("Watches")]
        [XmlArrayItem("Watched")]
        public DateTime[] Watches
        {
            get
            {
                if ((m_Watches == null) || (m_Watches.Length == 0))
                {
                    return (null);
                }

                for (Int32 i = 0; i < m_Watches.Length; i++)
                {
                    m_Watches[i] = m_Watches[i].ToUniversalTime();
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
}