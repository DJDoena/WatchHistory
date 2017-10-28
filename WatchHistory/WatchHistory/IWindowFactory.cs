using System;

namespace DoenaSoft.WatchHistory
{
    internal interface IWindowFactory
    {
        void OpenSelectUserWindow();

        void OpenMainWindow(String userName);

        void OpenSettingsWindow();

        void OpenIgnoreWindow(String userName);
    }
}
