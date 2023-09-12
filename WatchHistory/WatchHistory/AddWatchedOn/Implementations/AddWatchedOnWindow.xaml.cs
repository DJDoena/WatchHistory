namespace DoenaSoft.WatchHistory.AddWatchedOn.Implementations
{
    using System.Windows;
    using AbstractionLayer.UIServices;
    using WatchHistory.Implementations;

    public partial class AddWatchedOnWindow : Window
    {
        public AddWatchedOnWindow()
        {
            this.InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (IAddWatchedOnViewModel)this.DataContext;

            viewModel.Closing += this.OnClosing;
        }

        private void OnClosing(object sender, CloseEventArgs e)
        {
            var viewModel = (IAddWatchedOnViewModel)this.DataContext;

            viewModel.Closing -= this.OnClosing;

            this.DialogResult = e.Result == Result.OK;

            this.Close();
        }
    }
}