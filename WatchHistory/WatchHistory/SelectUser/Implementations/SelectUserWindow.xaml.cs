namespace DoenaSoft.WatchHistory.SelectUser.Implementations
{
    using System;
    using System.Windows;


    public partial class SelectUserWindow : Window
    {
        public SelectUserWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender
            , RoutedEventArgs e)
        {
            ISelectUserViewModel viewModel = (ISelectUserViewModel)DataContext;

            viewModel.Closing += OnViewModelClosing;
        }

        private void OnViewModelClosing(object sender
            , EventArgs e)
        {
            ISelectUserViewModel viewModel = (ISelectUserViewModel)DataContext;

            viewModel.Closing -= OnViewModelClosing;

            Close();
        }
    }
}