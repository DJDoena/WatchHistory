namespace DoenaSoft.WatchHistory.Manual.Implementations
{
    using System.Windows;
    using AbstractionLayer.UIServices;
    using WatchHistory.Implementations;

    public partial class ManualWindow : Window
    {
        public ManualWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender
            , RoutedEventArgs e)
        {
            var viewModel = (IManualViewModel)DataContext;

            viewModel.Closing += OnClosing;
        }

        private void OnClosing(object sender
            , CloseEventArgs e)
        {
            var viewModel = (IManualViewModel)DataContext;

            viewModel.Closing -= OnClosing;

            DialogResult = (e.Result == Result.OK) ? true : false;

            Close();
        }
    }
}