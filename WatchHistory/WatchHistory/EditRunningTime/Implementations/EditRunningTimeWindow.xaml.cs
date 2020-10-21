namespace DoenaSoft.WatchHistory.EditRunningTime.Implementations
{
    using System.Windows;
    using AbstractionLayer.UIServices;
    using WatchHistory.Implementations;

    public partial class EditRunningTimeWindow : Window
    {
        public EditRunningTimeWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            IEditRunningTimeViewModel viewModel = (IEditRunningTimeViewModel)DataContext;

            viewModel.Closing += OnClosing;
        }

        private void OnClosing(object sender, CloseEventArgs e)
        {
            IEditRunningTimeViewModel viewModel = (IEditRunningTimeViewModel)DataContext;

            viewModel.Closing -= OnClosing;

            DialogResult = e.Result == Result.OK;

            Close();
        }
    }
}