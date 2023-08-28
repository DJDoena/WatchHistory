using System;
using System.Collections.Generic;
using DoenaSoft.WatchHistory.Data;

namespace DoenaSoft.WatchHistory
{
    internal interface IWindowFactory
    {
        void OpenSelectUserWindow();

        void OpenMainWindow(string userName);

        void OpenSettingsWindow();

        void OpenIgnoreWindow(string userName, string filter);

        DateTime? OpenWatchedOnWindow();

        uint? OpenRunningTimeWindow(uint seconds);

        void OpenWatchesWindow(IEnumerable<Watch> watches);

        void OpenAddYoutubeLinkWindow(string userName);

        void OpenAddManualEntryWindow(string userName);

        string OpenEditTitleWindow(FileEntry entry);

        void OpenShowReportWindow(string userName);

        string OpenEditNoteWindow(string note);
    }
}