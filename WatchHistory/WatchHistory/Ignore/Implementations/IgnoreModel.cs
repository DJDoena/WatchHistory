namespace DoenaSoft.WatchHistory.Ignore.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Data;
    using WatchHistory.Implementations;

    internal sealed class IgnoreModel : ModelBase, IIgnoreModel
    {
        public IgnoreModel(IDataManager dataManager
            , String userName)
            : base(dataManager, userName)
        { }

        #region IIgnoreModel

        public IEnumerable<FileEntry> GetFiles()
        {
            IEnumerable<FileEntry> allFiles = _DataManager.GetFiles();

            IEnumerable<FileEntry> ignoredFiles = allFiles.Where(UserIgnores);

            IEnumerable<FileEntry> filteredFiles = ignoredFiles.Where(ContainsFilter).ToList();

            return (filteredFiles);
        }

        #endregion
    }
}