namespace DoenaSoft.WatchHistory.EditTitle.Implementations
{
    using System.Windows;
    using AbstractionLayer.UIServices;
    using WatchHistory.Implementations;

    public partial class EditTitleWindow : Window
    {
        public EditTitleWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (IEditTitleViewModel)DataContext;

            viewModel.Closing += OnClosing;
        }

        private void OnClosing(object sender, CloseEventArgs e)
        {
            var viewModel = (IEditTitleViewModel)DataContext;

            viewModel.Closing -= OnClosing;

            DialogResult = e.Result == Result.OK ? true : false;

            Close();
        }
    }
}