namespace DoenaSoft.WatchHistory.Ignore.Implementations
{
    using System.Collections.Generic;
    using System.Linq;
    using Data;
    using WatchHistory.Implementations;

    internal sealed class IgnoreModel : ModelBase, IIgnoreModel
    {
        public IgnoreModel(IDataManager dataManager
            , string userName)
            : base(dataManager, userName)
        { }

        #region IIgnoreModel

        public IEnumerable<FileEntry> GetFiles()
        {
            var allFiles = _dataManager.GetFiles();

            var ignoredFiles = allFiles.Where(UserIgnores);

            var filteredFiles = ignoredFiles.Where(ContainsFilter);

            var result = filteredFiles.ToList();

            return filteredFiles;
        }

        #endregion
    }
}