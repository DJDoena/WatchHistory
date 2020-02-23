namespace DoenaSoft.WatchHistory.AddYoutubeLink.Implementations
{
    using System.Windows;
    using AbstractionLayer.UIServices;
    using WatchHistory.Implementations;

    public partial class AddYoutubeLinkWindow : Window
    {
        public AddYoutubeLinkWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (IAddYoutubeLinkViewModel)DataContext;

            viewModel.Closing += OnClosing;
        }

        private void OnClosing(object sender, CloseEventArgs e)
        {
            var viewModel = (IAddYoutubeLinkViewModel)DataContext;

            viewModel.Closing -= OnClosing;

            DialogResult = e.Result == Result.OK ? true : false;

            Close();
        }
    }
}