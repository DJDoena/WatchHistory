namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Media;
    using AbstractionLayer.IOServices;
    using ToolBox.Extensions;
    using WatchHistory.Implementations;

    internal sealed class FileEntryViewModel : IFileEntryViewModel
    {
        private readonly string _userName;

        private readonly IDataManager _dataManager;

        private readonly IIOServices _ioServices;

        private User _user;

#pragma warning disable IDE1006 // Naming Styles
        private event PropertyChangedEventHandler _propertyChanged;
#pragma warning restore IDE1006 // Naming Styles

        public FileEntryViewModel(FileEntry entry, string userName, IDataManager dataManager, IIOServices ioServices)
        {
            FileEntry = entry;
            _userName = userName;
            _dataManager = dataManager;
            _ioServices = ioServices;

            _user = TryGetUser();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (_propertyChanged == null)
                {
                    if (_user != null)
                    {
                        _user.WatchesChanged += OnWatchesChanged;
                    }
                    else
                    {
                        FileEntry.UsersChanged += OnUsersChanged;
                    }

                    FileEntry.VideoLengthChanged += OnVideoLengthChanged;
                }

                _propertyChanged += value;
            }
            remove
            {
                _propertyChanged -= value;

                if (_propertyChanged == null)
                {
                    FileEntry.VideoLengthChanged -= OnVideoLengthChanged;

                    if (_user != null)
                    {
                        _user.WatchesChanged -= OnWatchesChanged;
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

        public string Name
        {
            get
            {
                if (FileEntry.TitleSpecified)
                {
                    return (FileEntry.Title);
                }

                string name = FileEntry.FullName;

                _dataManager.RootFolders.ForEach(folder => name = name.Replace(folder, string.Empty));

                name = name.Trim('\\', '/');

                name = name.Replace(Constants.Backslash, @"\");

                name = name.Replace(@"\", " > ");

                return name;
            }
        }

        public string LastWatched
        {
            get
            {
                var lastWatched = _dataManager.GetLastWatched(FileEntry, _userName);

                var text = string.Empty;

                if (lastWatched.Ticks != 0)
                {
                    text = ViewModelHelper.GetFormattedDateTime(lastWatched);
                }

                return text;
            }
        }

        public string CreationTime
        {
            get
            {
                var creationTime = FileEntry.GetCreationTime(_dataManager);

                var text = string.Empty;

                if (creationTime.Ticks != 0)
                {
                    text = ViewModelHelper.GetFormattedDateTime(creationTime);
                }

                return text;
            }
        }

        public string RunningTime
        {
            get
            {
                var runningTime = FileEntry.GetVideoLength(_dataManager);

                var text = string.Empty;

                if (runningTime > 0)
                {
                    text = ViewModelHelper.GetFormattedRunningTime(runningTime);
                }

                return text;
            }
        }

        public Brush Color => _ioServices.File.Exists(FileEntry.FullName) ? Brushes.Black : Brushes.Red;

        #endregion

        private User TryGetUser() => FileEntry.Users?.Where(item => item.UserName == _userName).FirstOrDefault();

        private void OnUsersChanged(object sender, EventArgs e)
        {
            _user = TryGetUser();

            if (_user != null)
            {
                if (_propertyChanged != null)
                {
                    FileEntry.UsersChanged -= OnUsersChanged;

                    _user.WatchesChanged += OnWatchesChanged;
                }
            }
        }

        private void OnWatchesChanged(object sender, EventArgs e) => RaisePropertyChanged(nameof(LastWatched));

        private void OnVideoLengthChanged(object sender, EventArgs e) => RaisePropertyChanged(nameof(RunningTime));

        private void RaisePropertyChanged(string attribute) => _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
    }
}