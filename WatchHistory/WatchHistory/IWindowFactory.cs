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

        Nullable<UInt32> OpenRunningTimeWindow(UInt32 seconds);

        void OpenWatchesWindow(IEnumerable<Watch> watches);
    }
}