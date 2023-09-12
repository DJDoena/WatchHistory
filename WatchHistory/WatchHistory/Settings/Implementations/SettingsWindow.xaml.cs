namespace DoenaSoft.WatchHistory.Settings.Implementations
{
    using System;
    using System.Windows;

    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            this.InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (ISettingsViewModel)this.DataContext;

            viewModel.Closing += this.OnClosing;
        }

        private void OnClosing(object sender, EventArgs e)
        {
            var viewModel = (ISettingsViewModel)this.DataContext;

            viewModel.Closing -= this.OnClosing;

            this.Close();
        }
    }
}