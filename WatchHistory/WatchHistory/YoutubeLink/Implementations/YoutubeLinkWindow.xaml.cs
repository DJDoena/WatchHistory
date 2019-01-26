namespace DoenaSoft.WatchHistory.YoutubeLink.Implementations
{
    using System;
    using System.Windows;
    using AbstractionLayer.UIServices;
    using WatchHistory.Implementations;

    public partial class YoutubeLinkWindow : Window
    {
        public YoutubeLinkWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(Object sender
            , RoutedEventArgs e)
        {
            IYoutubeLinkViewModel  viewModel = (IYoutubeLinkViewModel)DataContext;

            viewModel.Closing += OnClosing;
        }

        private void OnClosing(Object sender
            , CloseEventArgs e)
        {
            IYoutubeLinkViewModel viewModel = (IYoutubeLinkViewModel)DataContext;

            viewModel.Closing -= OnClosing;

            DialogResult = (e.Result == Result.OK) ? true : false;

            Close();
        }
    }
}