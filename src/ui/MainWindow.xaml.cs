using System.Windows;
using GMap.NET;

namespace nl.siwoc.RouteManager
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Set initial position to Amsterdam
            mapControl.Position = new PointLatLng(52.3676, 4.9041);
            mapControl.Zoom = 15;

            // Set the DataContext last
            DataContext = new MainViewModel(mapControl);
        }
    }
} 