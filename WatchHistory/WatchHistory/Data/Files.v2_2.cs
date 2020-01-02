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
        public Decimal Version;

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
        private User[] m_Users;

        private Nullable<DateTime> m_CreationTime;

        private UInt32 m_VideoLength;

        [XmlElement]
        public String FullName;

        [XmlAttribute]
        public DateTime CreationTime
        {
            get => m_CreationTime ?? new DateTime(0, DateTimeKind.Utc);
            set => m_CreationTime = value.Conform();
        }

        [XmlArray("Users")]
        [XmlArrayItem("User")]
        public User[] Users
        {
            get => m_Users;
            set
            {
                m_Users = value;

                UsersChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [XmlAttribute]
        public UInt32 VideoLength
        {
            get => m_VideoLength;
            set
            {
                if (m_VideoLength != value)
                {
                    m_VideoLength = value;

                    VideoLengthChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        [XmlIgnore]
        public Boolean VideoLengthSpecified
            => VideoLength > 0;

        [XmlAttribute]
        public String Title;

        [XmlIgnore]
        public Boolean TitleSpecified
            => !string.IsNullOrWhiteSpace(Title);

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
        private Watch[] m_Watches;

        [XmlAttribute]
        public String UserName;

        [XmlAttribute]
        public Boolean Ignore;

        [XmlIgnore]
        public Boolean IgnoreSpecified
            => Ignore;

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
        public XmlAttribute[] AnyAttributes;

        [XmlAnyElement]
        public XmlElement[] AnyElements;

        public event EventHandler WatchesChanged;

        public override Int32 GetHashCode()
            => UserName?.GetHashCode() ?? 0;

        public override Boolean Equals(Object obj)
            => Equals(obj as User);

        public Boolean Equals(User other)
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
        private DateTime m_Watched;

        [XmlAttribute]
        public DateTime Value
        {
            get => m_Watched;
            set => m_Watched = value.Conform();
        }

        [XmlAttribute]
        public String Source;

        [XmlIgnore]
        public Boolean SourceSpecified
            => !string.IsNullOrWhiteSpace(Source);

        [XmlAnyAttribute]
        public XmlAttribute[] AnyAttributes;

        [XmlAnyElement]
        public XmlElement[] AnyElements;

        public override Int32 GetHashCode()
            => Value.GetHashCode();

        public override Boolean Equals(Object obj)
            => (Equals(obj as Watch));

        public Boolean Equals(Watch other)
        {
            if (other == null)
            {
                return (false);
            }

            return ((Value == other.Value) && (Source == other.Source));
        }
    }
}