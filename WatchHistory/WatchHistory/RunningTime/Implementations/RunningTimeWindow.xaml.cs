namespace DoenaSoft.WatchHistory.RunningTime.Implementations
{
    using System;
    using System.Windows;
    using AbstractionLayer.UIServices;
    using WatchHistory.Implementations;

    public partial class RunningTimeWindow : Window
    {
        public RunningTimeWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(Object sender
            , RoutedEventArgs e)
        {
            IRunningTimeViewModel viewModel = (IRunningTimeViewModel)DataContext;

            viewModel.Closing += OnClosing;
        }

        private void OnClosing(Object sender
            , CloseEventArgs e)
        {
            IRunningTimeViewModel viewModel = (IRunningTimeViewModel)DataContext;

            viewModel.Closing -= OnClosing;

            DialogResult = (e.Result == Result.OK) ? true : false;

            Close();
        }
    }
}