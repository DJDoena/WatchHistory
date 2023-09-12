namespace DoenaSoft.WatchHistory.EditNote.Implementations
{
    using System.Windows;
    using AbstractionLayer.UIServices;
    using WatchHistory.Implementations;

    public partial class EditNoteWindow : Window
    {
        public EditNoteWindow()
        {
            this.InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (IEditNoteViewModel)this.DataContext;

            viewModel.Closing += this.OnClosing;
        }

        private void OnClosing(object sender, CloseEventArgs e)
        {
            var viewModel = (IEditNoteViewModel)this.DataContext;

            viewModel.Closing -= this.OnClosing;

            this.DialogResult = e.Result == Result.OK;

            this.Close();
        }
    }
}