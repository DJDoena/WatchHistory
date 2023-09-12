namespace DoenaSoft.WatchHistory.IgnoreEntry.Implementations
{
    using System.Collections.Generic;
    using System.Linq;
    using Data;
    using WatchHistory.Implementations;

    internal sealed class IgnoreEntryModel : ModelBase, IIgnoreEntryModel
    {
        public IgnoreEntryModel(IDataManager dataManager, string userName) : base(dataManager, userName)
        { }

        #region IIgnoreEntryModel

        public IEnumerable<FileEntry> GetFiles()
        {
            var allFiles = _dataManager.GetFiles();

            var ignoredFiles = allFiles.Where(this.UserIgnores);

            var filteredFiles = ignoredFiles.Where(this.ContainsFilter);

            var result = filteredFiles.ToList();

            return filteredFiles;
        }

        #endregion
    }
}