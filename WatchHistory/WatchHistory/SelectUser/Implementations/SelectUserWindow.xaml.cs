using System;
using System.Windows;

namespace DoenaSoft.WatchHistory.SelectUser.Implementations
{
    /// <summary>
    /// Interaction logic for SelectUserWindow.xaml
    /// </summary>
    public partial class SelectUserWindow : Window
    {
        public SelectUserWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(Object sender
            , RoutedEventArgs e)
        {
            ISelectUserViewModel viewModel = (ISelectUserViewModel)DataContext;

            viewModel.Closing += OnViewModelClosing;
        }

        private void OnViewModelClosing(Object sender
            , EventArgs e)
        {
            ISelectUserViewModel viewModel = (ISelectUserViewModel)DataContext;

            viewModel.Closing -= OnViewModelClosing;

            Close();
        }
    }
}
