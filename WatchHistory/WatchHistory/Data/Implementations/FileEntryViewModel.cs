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
        private readonly String _UserName;

        private readonly IDataManager _DataManager;

        private readonly IIOServices _IOServices;

        private User _User;

        private event PropertyChangedEventHandler _PropertyChanged;

        public FileEntryViewModel(FileEntry entry
            , String userName
            , IDataManager dataManager
            , IIOServices ioServices)
        {
            FileEntry = entry;
            _UserName = userName;
            _DataManager = dataManager;
            _IOServices = ioServices;

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
                if (FileEntry.TitleSpecified)
                {
                    return (FileEntry.Title);
                }

                String name = FileEntry.FullName;

                _DataManager.RootFolders.ForEach(folder => name = name.Replace(folder, String.Empty));

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
                DateTime lastWatched = _DataManager.GetLastWatched(FileEntry, _UserName);

                String text = String.Empty;

                if (lastWatched.Ticks != 0)
                {
                    text = ViewModelHelper.GetFormattedDateTime(lastWatched);
                }

                return (text);
            }
        }

        public String CreationTime
        {
            get
            {
                DateTime creationTime = FileEntry.GetCreationTime(_DataManager);

                String text = String.Empty;

                if (creationTime.Ticks != 0)
                {
                    text = ViewModelHelper.GetFormattedDateTime(creationTime);
                }

                return (text);
            }
        }

        public Brush Color
            => _IOServices.File.Exists(FileEntry.FullName) ? Brushes.Black : Brushes.Red;

        #endregion

        private User TryGetUser()
            => FileEntry.Users?.Where(item => item.UserName == _UserName).FirstOrDefault();

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