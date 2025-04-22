using System.Windows;
using GMap.NET;
using System.Windows.Input;
using System.Windows.Controls;

namespace nl.siwoc.RouteManager
{
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel => (MainViewModel)DataContext;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(mapControl);
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
    }
} 