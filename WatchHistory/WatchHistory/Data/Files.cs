namespace DoenaSoft.WatchHistory.Data
{
    using System;
    using System.Xml.Serialization;

    [XmlRoot]
    public sealed class Files
    {
        [XmlArray("Entries")]
        [XmlArrayItem("Entry")]
        public FileEntry[] Entries { get; set; }
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
                return (m_CreationTime ?? new DateTime(0));
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
                return (m_Watches);
            }
            set
            {
                m_Watches = value;

                WatchesChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler WatchesChanged;
    }
}