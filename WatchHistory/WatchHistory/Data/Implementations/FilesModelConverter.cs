namespace DoenaSoft.WatchHistory.Data.Implementations
{
    using System;

    internal static class FilesModelConverter
    {
        internal static Files Convert(v1_0.Files oldFiles)
        {
            Files newFiles = new Files();

            if (oldFiles.Entries?.Length > 0)
            {
                CopyEntries(oldFiles, newFiles);
            }

            return (newFiles);
        }

        private static void CopyEntries(v1_0.Files oldFiles
            , Files newFiles)
        {
            newFiles.Entries = new FileEntry[oldFiles.Entries.Length];

            for (Int32 entryIndex = 0; entryIndex < oldFiles.Entries.Length; entryIndex++)
            {
                CopyEntry(oldFiles, newFiles, entryIndex);
            }
        }

        private static void CopyEntry(v1_0.Files oldFiles
             , Files newFiles
             , Int32 entryIndex)
        {
            v1_0.FileEntry oldEntry = oldFiles.Entries[entryIndex];

            if (oldEntry != null)
            {
                FileEntry newEntry = new FileEntry();

                CopyEntry(oldEntry, newEntry);

                newFiles.Entries[entryIndex] = newEntry;
            }
        }

        private static void CopyEntry(v1_0.FileEntry oldEntry
            , FileEntry newEntry)
        {
            newEntry.CreationTime = oldEntry.CreationTime;
            newEntry.FullName = oldEntry.FullName;

            if (oldEntry.Users?.Length > 0)
            {
                CopyUsers(oldEntry, newEntry);
            }
        }

        private static void CopyUsers(v1_0.FileEntry oldEntry
            , FileEntry newEntry)
        {
            newEntry.Users = new User[oldEntry.Users.Length];

            for (Int32 userIndex = 0; userIndex < oldEntry.Users.Length; userIndex++)
            {
                CopyUser(oldEntry, newEntry, userIndex);
            }
        }

        private static void CopyUser(v1_0.FileEntry oldEntry
            , FileEntry newEntry
            , Int32 userIndex)
        {
            v1_0.User oldUser = oldEntry.Users[userIndex];

            if (oldUser != null)
            {
                User newUser = new User();

                CopyUser(oldUser, newUser);

                newEntry.Users[userIndex] = newUser;
            }
        }

        private static void CopyUser(v1_0.User oldUser
            , User newUser)
        {
            if (oldUser.IgnoreSpecified)
            {
                newUser.Ignore = oldUser.Ignore;
                newUser.IgnoreSpecified = true;
            }

            newUser.UserName = oldUser.UserName;

            if (oldUser.Watches?.Length > 0)
            {
                CopyWatches(oldUser, newUser);
            }
        }

        private static void CopyWatches(v1_0.User oldUser
            , User newUser)
        {
            newUser.Watches = new Watch[oldUser.Watches.Length];

            for (Int32 watchIndex = 0; watchIndex < oldUser.Watches.Length; watchIndex++)
            {
                DateTime oldWatch = oldUser.Watches[watchIndex];

                Watch newWatch = new Watch() { Value = oldWatch };

                newUser.Watches[watchIndex] = newWatch;
            }
        }
    }
}