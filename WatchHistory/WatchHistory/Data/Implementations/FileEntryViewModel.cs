namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Media;
    using AbstractionLayer.IOServices;

    internal sealed class FileEntryViewModel : IFileEntryViewModel
    {
        private readonly String UserName;

        private readonly IDataManager DataManager;

        private readonly IIOServices IOServices;

        private User m_User;

        private event PropertyChangedEventHandler m_PropertyChanged;

        public FileEntryViewModel(FileEntry entry
            , String userName
            , IDataManager dataManager
            , IIOServices ioServices)
        {
            FileEntry = entry;
            UserName = userName;
            DataManager = dataManager;
            IOServices = ioServices;

            m_User = TryGetUser();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (m_PropertyChanged == null)
                {
                    if (m_User != null)
                    {
                        m_User.WatchesChanged += OnWatchesChanged;
                    }
                    else
                    {
                        FileEntry.UsersChanged += OnUsersChanged;
                    }
                }

                m_PropertyChanged += value;
            }
            remove
            {
                m_PropertyChanged -= value;

                if (m_PropertyChanged == null)
                {
                    if (m_User != null)
                    {
                        m_User.WatchesChanged -= OnWatchesChanged;
                    }
                    else
                    {
                        FileEntry.UsersChanged -= OnUsersChanged;
                    }
                }
            }
        }

        #endregion

        #region IFileEntryViewModel

        public FileEntry FileEntry { get; private set; }

        public String Name
        {
            get
            {
                String name = FileEntry.FullName;

                foreach (String rootFolder in DataManager.RootFolders)
                {
                    name = name.Replace(rootFolder, String.Empty);
                }

                name = name.Trim('\\', '/');

                name = name.Replace(Constants.Backslash, @"\");

                name = name.Replace(@"\", " > ");

                return (name);
            }
        }

        public String LastWatched
        {
            get
            {
                DateTime lastWatched = DataManager.GetLastWatched(FileEntry, UserName);

                String text = String.Empty;

                if (lastWatched.Ticks != 0)
                {
                    text = $"{lastWatched.ToShortDateString()} {lastWatched.ToShortTimeString()}";
                }

                return (text);
            }
        }

        public String CreationTime
        {
            get
            {
                DateTime creationTime = FileEntry.GetCreationTime(DataManager);

                String text = String.Empty;

                if (creationTime.Ticks != 0)
                {
                    text = $"{creationTime.ToShortDateString()} {creationTime.ToShortTimeString()}";
                }

                return (text);
            }
        }

        public Brush Color
            => (IOServices.File.Exists(FileEntry.FullName) ? Brushes.Black : Brushes.Red);

        #endregion

        private User TryGetUser()
            => (FileEntry.Users?.Where(item => item.UserName == UserName).FirstOrDefault());

        private void OnUsersChanged(Object sender
            , EventArgs e)
        {
            m_User = TryGetUser();

            if (m_User != null)
            {
                if (m_PropertyChanged != null)
                {
                    FileEntry.UsersChanged -= OnUsersChanged;

                    m_User.WatchesChanged += OnWatchesChanged;
                }
            }
        }

        private void OnWatchesChanged(Object sender
            , EventArgs e)
        {
            RaisePropertyChanged(nameof(LastWatched));
        }

        private void RaisePropertyChanged(String attribute)
        {
            m_PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
        }
    }
}