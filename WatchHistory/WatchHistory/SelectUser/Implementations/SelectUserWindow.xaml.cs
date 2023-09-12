namespace DoenaSoft.WatchHistory.SelectUser.Implementations
{
    using System;
    using System.Windows;


    public partial class SelectUserWindow : Window
    {
        public SelectUserWindow()
        {
            this.InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var viewModel = (ISelectUserViewModel)this.DataContext;

            viewModel.Closing += this.OnViewModelClosing;
        }

        private void OnViewModelClosing(object sender, EventArgs e)
        {
            var viewModel = (ISelectUserViewModel)this.DataContext;

            viewModel.Closing -= this.OnViewModelClosing;

            this.Close();
        }
    }
}