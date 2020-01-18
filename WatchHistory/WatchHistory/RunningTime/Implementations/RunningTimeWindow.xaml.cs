namespace DoenaSoft.WatchHistory.RunningTime.Implementations
{
    using System.Windows;
    using AbstractionLayer.UIServices;
    using WatchHistory.Implementations;

    public partial class RunningTimeWindow : Window
    {
        public RunningTimeWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender
            , RoutedEventArgs e)
        {
            IRunningTimeViewModel viewModel = (IRunningTimeViewModel)DataContext;

            viewModel.Closing += OnClosing;
        }

        private void OnClosing(object sender
            , CloseEventArgs e)
        {
            IRunningTimeViewModel viewModel = (IRunningTimeViewModel)DataContext;

            viewModel.Closing -= OnClosing;

            DialogResult = (e.Result == Result.OK) ? true : false;

            Close();
        }
    }
}