namespace DoenaSoft.WatchHistory.EditNote.Implementations
{
    using System.Windows;
    using AbstractionLayer.UIServices;
    using WatchHistory.Implementations;

    public partial class EditNoteWindow : Window
    {
        public EditNoteWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (IEditNoteViewModel)DataContext;

            viewModel.Closing += OnClosing;
        }

        private void OnClosing(object sender, CloseEventArgs e)
        {
            var viewModel = (IEditNoteViewModel)DataContext;

            viewModel.Closing -= OnClosing;

            DialogResult = e.Result == Result.OK;

            Close();
        }
    }
}