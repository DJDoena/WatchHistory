namespace DoenaSoft.WatchHistory.Ignore.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Data;
    using ToolBox.Extensions;

    internal sealed class IgnoreModel : IIgnoreModel
    {
        private readonly IDataManager _DataManager;

        private readonly String _UserName;

        private String _Filter;

        private event EventHandler _FilesChanged;

        public IgnoreModel(IDataManager dataManager
            , String userName)
        {
            _DataManager = dataManager;
            _UserName = userName;
        }

        #region IIgnoreModel

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

        public IEnumerable<FileEntry> GetFiles()
        {
            IEnumerable<FileEntry> allFiles = _DataManager.GetFiles();

            IEnumerable<FileEntry> ignoredFiles = allFiles.Where(UserIgnores);

            IEnumerable<FileEntry> filteredFiles = ignoredFiles.Where(ContainsFilter).ToList();

            return (filteredFiles);
        }

        #endregion

        #region UserIgnores

        private Boolean UserIgnores(FileEntry file)
            => file.Users?.HasItemsWhere(UserIgnores) == true;

        private Boolean UserIgnores(User user)
            => (user.UserName == _UserName) && (user.Ignore);

        #endregion

        #region ContainsFilter

        private Boolean ContainsFilter(FileEntry file)
            => ContainsFilter(file, Filter.Trim().Split(' '));

        private static Boolean ContainsFilter(FileEntry file
           , IEnumerable<String> filters)
            => filters.All(filter => ContainsFilter(file, filter));

        private static Boolean ContainsFilter(FileEntry file
            , String filter)
            => file.FullName.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) != -1;

        #endregion

        private void OnDataManagerFilesChanged(Object sender
            , EventArgs e)
            => RaiseFilesChanged(e);

        private void RaiseFilesChanged(EventArgs e)
            => _FilesChanged?.Invoke(this, e);
    }
}