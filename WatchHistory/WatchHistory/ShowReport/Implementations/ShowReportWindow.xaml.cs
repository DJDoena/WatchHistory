namespace DoenaSoft.WatchHistory.ShowReport.Implementations
{
    using System.Windows;
    using AbstractionLayer.UIServices;
    using WatchHistory.Implementations;

    public partial class ShowReportWindow : Window
    {
        public ShowReportWindow()
        {
            this.InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (IShowReportViewModel)this.DataContext;

            viewModel.Closing += this.OnClosing;
        }

        private void OnClosing(object sender
            , CloseEventArgs e)
        {
            var viewModel = (IShowReportViewModel)this.DataContext;

            viewModel.Closing -= this.OnClosing;

            this.DialogResult = e.Result == Result.OK;

            this.Close();
        }
    }
}