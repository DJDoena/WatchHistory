namespace DoenaSoft.WatchHistory.Ignore.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Data;
    using ToolBox.Extensions;

    internal sealed class IgnoreModel : IIgnoreModel
    {
        private readonly IDataManager DataManager;

        private readonly String UserName;

        private String m_Filter;

        private event EventHandler m_FilesChanged;

        public IgnoreModel(IDataManager dataManager
            , String userName)
        {
            DataManager = dataManager;
            UserName = userName;
        }

        #region IIgnoreModel

        public String Filter
        {
            get
            {
                return (m_Filter ?? String.Empty);
            }
            set
            {
                if (value != m_Filter)
                {
                    m_Filter = value;

                    RaiseFilesChanged(EventArgs.Empty);
                }
            }
        }

        public event EventHandler FilesChanged
        {
            add
            {
                if (m_FilesChanged == null)
                {
                    DataManager.FilesChanged += OnDataManagerFilesChanged;
                }

                m_FilesChanged += value;
            }
            remove
            {
                m_FilesChanged -= value;

                if (m_FilesChanged == null)
                {
                    DataManager.FilesChanged -= OnDataManagerFilesChanged;
                }
            }
        }

        public IEnumerable<FileEntry> GetFiles()
        {
            IEnumerable<FileEntry> files = DataManager.GetFiles();

            files = files.Where(UserIgnores);

            files = files.Where(ContainsFilter);

            files = files.ToList();

            return (files);
        }

        #endregion

        #region UserIgnores

        private Boolean UserIgnores(FileEntry file)
            => (file.Users?.HasItemsWhere(UserIgnores) == true);

        private Boolean UserIgnores(User user)
            => ((user.UserName == UserName) && (user.Ignore));

        #endregion

        #region ContainsFilter

        private Boolean ContainsFilter(FileEntry file)
            => (ContainsFilter(file, Filter.Trim().Split(' ')));

        private static Boolean ContainsFilter(FileEntry file
           , IEnumerable<String> filters)
            => (filters.All(filter => ContainsFilter(file, filter)));

        private static Boolean ContainsFilter(FileEntry file
            , String filter)
            => (file.FullName.IndexOf(filter, StringComparison.InvariantCultureIgnoreCase) != -1);

        #endregion

        private void OnDataManagerFilesChanged(Object sender
            , EventArgs e)
        {
            RaiseFilesChanged(e);
        }

        private void RaiseFilesChanged(EventArgs e)
        {
            m_FilesChanged?.Invoke(this, e);
        }
    }
}