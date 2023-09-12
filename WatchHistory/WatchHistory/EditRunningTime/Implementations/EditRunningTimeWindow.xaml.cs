namespace DoenaSoft.WatchHistory.EditRunningTime.Implementations
{
    using System.Windows;
    using AbstractionLayer.UIServices;
    using WatchHistory.Implementations;

    public partial class EditRunningTimeWindow : Window
    {
        public EditRunningTimeWindow()
        {
            this.InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (IEditRunningTimeViewModel)this.DataContext;

            viewModel.Closing += this.OnClosing;
        }

        private void OnClosing(object sender, CloseEventArgs e)
        {
            var viewModel = (IEditRunningTimeViewModel)this.DataContext;

            viewModel.Closing -= this.OnClosing;

            this.DialogResult = e.Result == Result.OK;

            this.Close();
        }
    }
}