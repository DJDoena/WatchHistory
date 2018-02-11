namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Media;
    using AbstractionLayer.IOServices;
    using ToolBox.Extensions;

    internal sealed class FileEntryViewModel : IFileEntryViewModel
    {
        private readonly String UserName;

        private readonly IDataManager DataManager;

        private readonly IIOServices IOServices;

        private User _User;

        private event PropertyChangedEventHandler _PropertyChanged;

        public FileEntryViewModel(FileEntry entry
            , String userName
            , IDataManager dataManager
            , IIOServices ioServices)
        {
            FileEntry = entry;
            UserName = userName;
            DataManager = dataManager;
            IOServices = ioServices;

            _User = TryGetUser();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (_PropertyChanged == null)
                {
                    if (_User != null)
                    {
                        _User.WatchesChanged += OnWatchesChanged;
                    }
                    else
                    {
                        FileEntry.UsersChanged += OnUsersChanged;
                    }
                }

                _PropertyChanged += value;
            }
            remove
            {
                _PropertyChanged -= value;

                if (_PropertyChanged == null)
                {
                    if (_User != null)
                    {
                        _User.WatchesChanged -= OnWatchesChanged;
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

                DataManager.RootFolders.ForEach(folder => name = name.Replace(folder, String.Empty));

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
            => IOServices.File.Exists(FileEntry.FullName) ? Brushes.Black : Brushes.Red;

        #endregion

        private User TryGetUser()
            => FileEntry.Users?.Where(item => item.UserName == UserName).FirstOrDefault();

        private void OnUsersChanged(Object sender
            , EventArgs e)
        {
            _User = TryGetUser();

            if (_User != null)
            {
                if (_PropertyChanged != null)
                {
                    FileEntry.UsersChanged -= OnUsersChanged;

                    _User.WatchesChanged += OnWatchesChanged;
                }
            }
        }

        private void OnWatchesChanged(Object sender
            , EventArgs e)
            => RaisePropertyChanged(nameof(LastWatched));

        private void RaisePropertyChanged(String attribute)
            => _PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
    }
}