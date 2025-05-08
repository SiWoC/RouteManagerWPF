using System.Windows;
using GMap.NET;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Threading;

namespace nl.siwoc.RouteManager.ui
{
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel => (MainViewModel)DataContext;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(mapControl);
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!ViewModel.ConfirmUnsavedChanges())
            {
                e.Cancel = true;
            }
        }

        private void RoutePointsDataGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.RoutePointsDataGrid_HandleDragStart(e);
        }

        private void RoutePointsDataGrid_Drop(object sender, DragEventArgs e)
        {
            ViewModel.RoutePointsDataGrid_HandleDrop(e);
        }

        private void RoutePointsDataGrid_DragOver(object sender, DragEventArgs e)
        {
            ViewModel.RoutePointsDataGrid_HandleDragOver(e);
        }

        // selecting next/prev row with arrow keys
        // and prevent arrow keys from navigating outside the datagrid
        private void RoutePointsDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                e.Handled = true;
                var grid = (DataGrid)sender;
                var currentIndex = grid.SelectedIndex;
                var newIndex = e.Key == Key.Up ? currentIndex - 1 : currentIndex + 1;

                if (newIndex >= 0 && newIndex < grid.Items.Count)
                {
                    grid.SelectedIndex = newIndex;
                    grid.ScrollIntoView(grid.Items[newIndex]);
                }
            }
        }

        public void ScrollToSelectedPoint()
        {
            // don't scroll to previous point, wait for the new point to be selected
            Dispatcher.BeginInvoke(new Action(() => {
                if (RoutePointsDataGrid.SelectedItem != null)
                {
                    RoutePointsDataGrid.ScrollIntoView(RoutePointsDataGrid.SelectedItem);
                    RoutePointsDataGrid.Focus();
                }
            }));
        }
    }
} 