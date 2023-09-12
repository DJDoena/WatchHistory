namespace DoenaSoft.WatchHistory.AddYoutubeLink.Implementations
{
    using System.Windows;
    using AbstractionLayer.UIServices;
    using WatchHistory.Implementations;

    public partial class AddYoutubeLinkWindow : Window
    {
        public AddYoutubeLinkWindow()
        {
            this.InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (IAddYoutubeLinkViewModel)this.DataContext;

            viewModel.Closing += this.OnClosing;
        }

        private void OnClosing(object sender, CloseEventArgs e)
        {
            var viewModel = (IAddYoutubeLinkViewModel)this.DataContext;

            viewModel.Closing -= this.OnClosing;

            this.DialogResult = e.Result == Result.OK;

            this.Close();
        }
    }
}