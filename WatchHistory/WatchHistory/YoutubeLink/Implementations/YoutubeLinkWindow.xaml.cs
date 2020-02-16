namespace DoenaSoft.WatchHistory.YoutubeLink.Implementations
{
    using System.Windows;
    using AbstractionLayer.UIServices;
    using WatchHistory.Implementations;

    public partial class YoutubeLinkWindow : Window
    {
        public YoutubeLinkWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender
            , RoutedEventArgs e)
        {
            var viewModel = (IYoutubeLinkViewModel)DataContext;

            viewModel.Closing += OnClosing;
        }

        private void OnClosing(object sender
            , CloseEventArgs e)
        {
            var viewModel = (IYoutubeLinkViewModel)DataContext;

            viewModel.Closing -= OnClosing;

            DialogResult = (e.Result == Result.OK) ? true : false;

            Close();
        }
    }
}