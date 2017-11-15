namespace DoenaSoft.WatchHistory
{
    using System;

    internal interface IWindowFactory
    {
        void OpenSelectUserWindow();

        void OpenMainWindow(String userName);

        void OpenSettingsWindow();

        void OpenIgnoreWindow(String userName);
    }
}