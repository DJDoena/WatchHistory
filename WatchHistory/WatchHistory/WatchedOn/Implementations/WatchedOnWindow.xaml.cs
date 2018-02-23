namespace DoenaSoft.WatchHistory.WatchedOn.Implementations
{
    using System;
    using System.Windows;
    using AbstractionLayer.UIServices;

    public partial class WatchedOnWindow : Window
    {
        public WatchedOnWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(Object sender
            , RoutedEventArgs e)
        {
            IWatchedOnViewModel viewModel = (IWatchedOnViewModel)DataContext;

            viewModel.Closing += OnClosing;
        }

        private void OnClosing(Object sender
            , CloseEventArgs e)
        {
            IWatchedOnViewModel viewModel = (IWatchedOnViewModel)DataContext;

            viewModel.Closing -= OnClosing;

            DialogResult = (e.Result == Result.OK) ? true : false;

            Close();
        }
    }
}