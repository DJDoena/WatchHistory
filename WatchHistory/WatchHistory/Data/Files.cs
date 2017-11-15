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

    public class FileEntry
    {
        private User[] m_Users;

        private Nullable<DateTime> m_CreationTime;

        [XmlElement]
        public String FullName { get; set; }

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

        internal DateTime GetCreationTime(IDataManager dataManager)
        {
            if (m_CreationTime.HasValue == false)
            {
                m_CreationTime = dataManager.GetCreationTime(this);
            }

            return (m_CreationTime.Value);
        }
    }

    public class User
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