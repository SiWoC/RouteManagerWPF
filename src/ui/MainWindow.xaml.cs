using System.Windows;
using GMap.NET;

namespace nl.siwoc.RouteManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Set the DataContext last
            DataContext = new MainViewModel(mapControl);
        }
    }
} 