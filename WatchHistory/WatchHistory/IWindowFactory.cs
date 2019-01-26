namespace DoenaSoft.WatchHistory
{
    using System;
    using System.Collections.Generic;
    using Data;

    internal interface IWindowFactory
    {
        void OpenSelectUserWindow();

        void OpenMainWindow(string userName);

        void OpenSettingsWindow();

        void OpenIgnoreWindow(string userName);

        DateTime? OpenWatchedOnWindow();

        uint? OpenRunningTimeWindow(uint seconds);

        void OpenWatchesWindow(IEnumerable<Watch> watches);

        void OpenAddYoutubeLinkVideo(string userName);
    }
}