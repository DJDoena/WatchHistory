namespace DoenaSoft.WatchHistory.ShowReport.Implementations
{
    using System.Windows;
    using AbstractionLayer.UIServices;
    using WatchHistory.Implementations;

    public partial class ShowReportWindow : Window
    {
        public ShowReportWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (IShowReportViewModel)DataContext;

            viewModel.Closing += OnClosing;
        }

        private void OnClosing(object sender
            , CloseEventArgs e)
        {
            var viewModel = (IShowReportViewModel)DataContext;

            viewModel.Closing -= OnClosing;

            DialogResult = e.Result == Result.OK ? true : false;

            Close();
        }
    }
}