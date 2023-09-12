namespace DoenaSoft.WatchHistory.AddManualEntry.Implementations
{
    using System.Windows;
    using AbstractionLayer.UIServices;
    using WatchHistory.Implementations;

    public partial class AddManualEntryWindow : Window
    {
        public AddManualEntryWindow()
        {
            this.InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (IAddManualEntryViewModel)this.DataContext;

            viewModel.Closing += this.OnClosing;
        }

        private void OnClosing(object sender, CloseEventArgs e)
        {
            var viewModel = (IAddManualEntryViewModel)this.DataContext;

            viewModel.Closing -= this.OnClosing;

            this.DialogResult = e.Result == Result.OK
                ? true
                : false;

            this.Close();
        }
    }
}