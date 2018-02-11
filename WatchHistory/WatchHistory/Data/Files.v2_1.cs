namespace DoenaSoft.WatchHistory.Data
{
    using System;
    using System.Diagnostics;
    using System.Xml;
    using System.Xml.Serialization;
    using ToolBox.Extensions;

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
            Version = 2.1m;
        }
    }

    [DebuggerDisplay("File: {FullName}")]
    public sealed partial class FileEntry
    {
        private User[] m_Users;

        private Nullable<DateTime> m_CreationTime;

        [XmlElement]
        public String FullName { get; set; }

        [XmlIgnore]
        public String Key
            => (FullName?.ToLower() ?? String.Empty);

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
        public UInt32 VideoLength { get; set; }

        [XmlIgnore]
        public Boolean VideoLengthSpecified { get; set; }

        [XmlAnyAttribute]
        public XmlAttribute[] AnyAttributes { get; set; }

        [XmlAnyElement]
        public XmlElement[] AnyElements { get; set; }

        public event EventHandler UsersChanged;
    }

    [DebuggerDisplay("User: {UserName}")]
    public sealed class User : IEquatable<User>
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

        public override Int32 GetHashCode()
            => (UserName?.GetHashCode() ?? 0);

        public override Boolean Equals(Object obj)
            => (Equals(obj as User));

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
            get
            {
                return m_Watched;
            }
            set
            {
                m_Watched = value.Conform();
            }
        }

        [XmlAttribute]
        public String Source { get; set; }

        [XmlIgnore]
        public Boolean SourceSpecified
            => (Source.IsNotEmpty());

        [XmlAnyAttribute]
        public XmlAttribute[] AnyAttributes { get; set; }

        [XmlAnyElement]
        public XmlElement[] AnyElements { get; set; }

        public override Int32 GetHashCode()
            => (Value.GetHashCode());

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