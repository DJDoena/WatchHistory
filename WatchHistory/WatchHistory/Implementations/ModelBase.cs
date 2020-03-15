namespace DoenaSoft.WatchHistory.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Data;
    using ToolBox.Extensions;

    internal abstract class ModelBase
    {
        protected readonly IDataManager _dataManager;

        protected readonly string _userName;

        private string _filter;

        private bool _searchInPath;

#pragma warning disable IDE1006 // Naming Styles
        private event EventHandler _filesChanged;
#pragma warning restore IDE1006 // Naming Styles

        protected ModelBase(IDataManager dataManager
            , string userName)
        {
            _dataManager = dataManager;
            _userName = userName;
            _searchInPath = false;
        }

        public string Filter
        {
            get => _filter ?? string.Empty;
            set
            {
                if (value != _filter)
                {
                    _filter = value;

                    RaiseFilesChanged(EventArgs.Empty);
                }
            }
        }

        public bool SearchInPath
        {
            get => _searchInPath;
            set
            {
                if (value != _searchInPath)
                {
                    _searchInPath = value;

                    RaiseFilesChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler FilesChanged
        {
            add
            {
                if (_filesChanged == null)
                {
                    _dataManager.FilesChanged += OnDataManagerFilesChanged;
                }

                _filesChanged += value;
            }
            remove
            {
                _filesChanged -= value;

                if (_filesChanged == null)
                {
                    _dataManager.FilesChanged -= OnDataManagerFilesChanged;
                }
            }
        }

        #region UserIgnores

        protected bool UserIgnores(FileEntry file) => file.Users?.HasItemsWhere(UserIgnores) == true;

        private bool UserIgnores(User user) => IsUser(user) && user.Ignore;

        #endregion

        protected bool IsUser(User user) => user.UserName == _userName;

        #region ContainsFilter

        protected bool ContainsFilter(FileEntry file) => ContainsFilter(file, Filter.Trim().Split(' '));

        private bool ContainsFilter(FileEntry file, IEnumerable<string> filters) => filters.All(filter => ContainsFilter(file, filter));

        private bool ContainsFilter(FileEntry file, string filter)
        {
            var contains = file.TitleSpecified
                  ? ContainsFilter(file.Title, filter)
                  : ContainsFilterInFileName(file, filter);

            if (!contains && SearchInPath)
            {
                contains = ContainsFilter(file.FullName, filter);
            }

            return contains;
        }

        private bool ContainsFilterInFileName(FileEntry file, string filter)
        {
            var fileName = CutRootFolders(file.FullName);

            var contains = ContainsFilter(fileName, filter);

            return contains;
        }

        private static bool ContainsFilter(string text, string filter) => text.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) != -1;

        private string CutRootFolders(string fullName)
        {
            _dataManager.RootFolders.ForEach(folder => fullName = fullName.Replace(folder, string.Empty));

            return fullName;
        }

        #endregion

        private void OnDataManagerFilesChanged(object sender, EventArgs e) => RaiseFilesChanged(e);

        protected void RaiseFilesChanged(EventArgs e) => _filesChanged?.Invoke(this, e);
    }
}
