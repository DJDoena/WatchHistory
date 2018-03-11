namespace DoenaSoft.WatchHistory
{
    using System;
    using System.Collections.Generic;
    using Data;

    internal interface IWindowFactory
    {
        void OpenSelectUserWindow();

        void OpenMainWindow(String userName);

        void OpenSettingsWindow();

        void OpenIgnoreWindow(String userName);

        Nullable<DateTime> OpenWatchedOnWindow();

        void OpenWatchesWindow(IEnumerable<Watch> watches);
    }
}