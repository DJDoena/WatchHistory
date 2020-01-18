namespace DoenaSoft.WatchHistory.WatchedOn.Implementations
{
    using System.Windows;
    using AbstractionLayer.UIServices;
    using WatchHistory.Implementations;

    public partial class WatchedOnWindow : Window
    {
        public WatchedOnWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender
            , RoutedEventArgs e)
        {
            IWatchedOnViewModel viewModel = (IWatchedOnViewModel)DataContext;

            viewModel.Closing += OnClosing;
        }

        private void OnClosing(object sender
            , CloseEventArgs e)
        {
            IWatchedOnViewModel viewModel = (IWatchedOnViewModel)DataContext;

            viewModel.Closing -= OnClosing;

            DialogResult = (e.Result == Result.OK) ? true : false;

            Close();
        }
    }
}