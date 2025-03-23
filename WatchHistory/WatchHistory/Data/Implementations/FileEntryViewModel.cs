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
            this.Entry = entry;
            _userName = userName;
            _dataManager = dataManager;
            _ioServices = ioServices;

            _user = this.TryGetUser();
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
                        _user.WatchesChanged += this.OnWatchesChanged;
                    }
                    else
                    {
                        this.Entry.UsersChanged += this.OnUsersChanged;
                    }

                    this.Entry.VideoLengthChanged += this.OnVideoLengthChanged;
                }

                _propertyChanged += value;
            }
            remove
            {
                _propertyChanged -= value;

                if (_propertyChanged == null)
                {
                    this.Entry.VideoLengthChanged -= this.OnVideoLengthChanged;

                    if (_user != null)
                    {
                        _user.WatchesChanged -= this.OnWatchesChanged;
                    }
                    else
                    {
                        this.Entry.UsersChanged -= this.OnUsersChanged;
                    }
                }
            }
        }

        #endregion

        #region IFileEntryViewModel

        public FileEntry Entry { get; private set; }

        public string Name
        {
            get
            {
                if (this.Entry.TitleSpecified)
                {
                    return (this.Entry.Title);
                }

                var fileInfo = _ioServices.GetFile(this.Entry.FullName);

                var folderName = fileInfo.FolderName;

                _dataManager.RootFolders.ForEach(folder => folderName = folderName.Replace(folder, string.Empty));

                folderName = folderName.Replace('\\', '/');

                folderName = folderName.Replace(Constants.Backslash, "/");

                folderName = folderName.Trim('/', ' ', '\t');

                folderName = folderName.Replace("/", " > ");

                if (!string.IsNullOrWhiteSpace(folderName))
                {
                    return $"{folderName} > {fileInfo.NameWithoutExtension}";
                }
                else
                {
                    return fileInfo.NameWithoutExtension;
                }
            }
        }

        public string LastWatched
        {
            get
            {
                var lastWatched = _dataManager.GetLastWatched(this.Entry, _userName);

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
                var creationTime = this.Entry.GetCreationTime(_dataManager);

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
                var runningTime = this.Entry.GetVideoLength(_dataManager);

                var text = string.Empty;

                if (runningTime > 0)
                {
                    text = ViewModelHelper.GetFormattedRunningTime(runningTime);
                }

                return text;
            }
        }

        public Brush Color => _ioServices.File.Exists(this.Entry.FullName)
            ? Brushes.Black
            : Brushes.Red;

        public string FullPath => this.Entry.FullName;

        #endregion

        private User TryGetUser() => this.Entry.Users?.Where(item => item.UserName == _userName).FirstOrDefault();

        private void OnUsersChanged(object sender, EventArgs e)
        {
            _user = this.TryGetUser();

            if (_user != null)
            {
                if (_propertyChanged != null)
                {
                    this.Entry.UsersChanged -= this.OnUsersChanged;

                    _user.WatchesChanged += this.OnWatchesChanged;
                }
            }
        }

        private void OnWatchesChanged(object sender, EventArgs e) => this.RaisePropertyChanged(nameof(this.LastWatched));

        private void OnVideoLengthChanged(object sender, EventArgs e) => this.RaisePropertyChanged(nameof(this.RunningTime));

        private void RaisePropertyChanged(string attribute) => _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(attribute));
    }
}