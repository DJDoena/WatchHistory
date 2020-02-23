namespace DoenaSoft.WatchHistory.Settings.Implementations
{
    using System;
    using System.Windows;

    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ISettingsViewModel viewModel = (ISettingsViewModel)DataContext;

            viewModel.Closing += OnClosing;
        }

        private void OnClosing(object sender, EventArgs e)
        {
            ISettingsViewModel viewModel = (ISettingsViewModel)DataContext;

            viewModel.Closing -= OnClosing;

            Close();
        }
    }
}