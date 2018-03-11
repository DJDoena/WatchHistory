namespace DoenaSoft.WatchHistory.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Data;
    using ToolBox.Extensions;

    internal abstract class ModelBase
    {
        protected readonly IDataManager _DataManager;

        protected readonly String _UserName;

        private String _Filter;

        private event EventHandler _FilesChanged;

        protected ModelBase(IDataManager dataManager
            , String userName)
        {
            _DataManager = dataManager;
            _UserName = userName;
        }

        public String Filter
        {
            get => _Filter ?? String.Empty;
            set
            {
                if (value != _Filter)
                {
                    _Filter = value;

                    RaiseFilesChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler FilesChanged
        {
            add
            {
                if (_FilesChanged == null)
                {
                    _DataManager.FilesChanged += OnDataManagerFilesChanged;
                }

                _FilesChanged += value;
            }
            remove
            {
                _FilesChanged -= value;

                if (_FilesChanged == null)
                {
                    _DataManager.FilesChanged -= OnDataManagerFilesChanged;
                }
            }
        }

        #region UserIgnores

        protected Boolean UserIgnores(FileEntry file)
            => file.Users?.HasItemsWhere(UserIgnores) == true;

        private Boolean UserIgnores(User user)
            => (IsUser(user)) && (user.Ignore);

        #endregion

        protected Boolean IsUser(Data.User user)
            => user.UserName == _UserName;

        #region ContainsFilter

        protected Boolean ContainsFilter(FileEntry file)
            => ContainsFilter(file, Filter.Trim().Split(' '));

        private Boolean ContainsFilter(FileEntry file
           , IEnumerable<String> filters)
            => filters.All(filter => ContainsFilter(file, filter));

        private Boolean ContainsFilter(FileEntry file
            , String filter)
            => file.TitleSpecified
                ? ContainsFilter(file.Title, filter)
                : ContainsFilterInFileName(file, filter);

        private Boolean ContainsFilterInFileName(FileEntry file, String filter)
        {
            String fileName = CutRootFolders(file.FullName);

            Boolean contains = ContainsFilter(fileName, filter);

            return (contains);
        }

        private static Boolean ContainsFilter(String text, String filter)
            => text.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) != -1;

        private String CutRootFolders(String fullName)
        {
            _DataManager.RootFolders.ForEach(folder => fullName = fullName.Replace(folder, String.Empty));

            return (fullName);
        }

        #endregion

        private void OnDataManagerFilesChanged(Object sender
            , EventArgs e)
            => RaiseFilesChanged(e);

        protected void RaiseFilesChanged(EventArgs e)
            => _FilesChanged?.Invoke(this, e);
    }
}
