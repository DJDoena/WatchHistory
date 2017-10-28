using System;
using System.Windows;

namespace DoenaSoft.WatchHistory.Settings.Implementations
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(Object sender
            , RoutedEventArgs e)
        {
            ISettingsViewModel viewModel = (ISettingsViewModel)DataContext;

            viewModel.Closing += OnClosing;
        }

        private void OnClosing(Object sender
            , EventArgs e)
        {
            ISettingsViewModel viewModel = (ISettingsViewModel)DataContext;

            viewModel.Closing -= OnClosing;

            Close();
        }
    }
}
