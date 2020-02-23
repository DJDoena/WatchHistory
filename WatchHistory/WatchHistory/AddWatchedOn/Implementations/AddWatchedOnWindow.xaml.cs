namespace DoenaSoft.WatchHistory.AddWatchedOn.Implementations
{
    using System.Windows;
    using AbstractionLayer.UIServices;
    using WatchHistory.Implementations;

    public partial class AddWatchedOnWindow : Window
    {
        public AddWatchedOnWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (IAddWatchedOnViewModel)DataContext;

            viewModel.Closing += OnClosing;
        }

        private void OnClosing(object sender
            , CloseEventArgs e)
        {
            var viewModel = (IAddWatchedOnViewModel)DataContext;

            viewModel.Closing -= OnClosing;

            DialogResult = e.Result == Result.OK ? true : false;

            Close();
        }
    }
}