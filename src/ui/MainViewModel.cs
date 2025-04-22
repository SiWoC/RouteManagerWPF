using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using GMap.NET;
using GMap.NET.MapProviders;
using CommunityToolkit.Mvvm.Input;
using GMap.NET.WindowsPresentation;
using System.Reflection;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Windows.Controls;
using System.Linq;

namespace nl.siwoc.RouteManager
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string statusMessage = "Ready";
        private ObservableCollection<RoutePoint> routePoints = new ObservableCollection<RoutePoint>();
        private PointLatLng center;
        private GMapProvider mapProvider = OpenStreetMapProvider.Instance;
        private readonly MapControlWrapper mapControl;
        private RoutePoint selectedPoint;
        private RoutePoint draggedItem;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<GMapProvider> MapProviders { get; } = new ObservableCollection<GMapProvider>();

        public GMapProvider MapProvider
        {
            get => mapProvider;
            set
            {
                if (mapProvider != value)
                {
                    System.Diagnostics.Debug.WriteLine($"MainViewModel MapProvider changing from {mapProvider?.Name ?? "null"} to {value?.Name ?? "null"}");
                    mapProvider = value;
                    OnPropertyChanged();
                    
                    // Update the MapControlWrapper
                    if (mapControl != null)
                    {
                        mapControl.MapProvider = value;
                        System.Diagnostics.Debug.WriteLine($"MapControlWrapper MapProvider set to: {value?.Name ?? "null"}");
                    }

                    // Save the provider
                    Settings.SaveMapProvider(value);
                }
            }
        }

        public string StatusMessage
        {
            get => statusMessage;
            set
            {
                if (statusMessage != value)
                {
                    statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public PointLatLng Center
        {
            get => center;
            set
            {
                if (center != value)
                {
                    center = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<RoutePoint> RoutePoints
        {
            get => routePoints;
            set
            {
                if (routePoints != value)
                {
                    routePoints = value;
                    OnPropertyChanged();
                }
            }
        }

        public RoutePoint SelectedPoint
        {
            get => selectedPoint;
            set
            {
                if (selectedPoint != value)
                {
                    selectedPoint = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand NewRouteCommand { get; }
        public ICommand OpenRouteCommand { get; }
        public ICommand SaveRouteCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand FitToRouteCommand { get; }
        public ICommand AddPointAtLocationCommand { get; }
        public ICommand DeletePointCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand DragStartCommand { get; }
        public ICommand DropCommand { get; }
        public ICommand DragOverCommand { get; }

        public MainViewModel(MapControlWrapper mapControl)
        {
            this.mapControl = mapControl;
            this.mapControl.ShowCenter = false;
            center = new PointLatLng(Settings.LoadStartLatitude(), Settings.LoadStartLongitude());

            // Initialize commands
            NewRouteCommand = new RelayCommand(ExecuteNewRoute);
            OpenRouteCommand = new RelayCommand(ExecuteOpenRoute);
            SaveRouteCommand = new RelayCommand(ExecuteSaveRoute);
            ExitCommand = new RelayCommand(ExecuteExit);
            FitToRouteCommand = new RelayCommand(ExecuteFitToRoute);
            AddPointAtLocationCommand = new RelayCommand(ExecuteAddPointAtLocation);
            DeletePointCommand = new RelayCommand(ExecuteDeletePoint);
            SettingsCommand = new RelayCommand(ExecuteSettings);

            // Initialize map providers
            foreach (var provider in MapControlWrapper.GetAllMapProviders())
            {
                MapProviders.Add(provider);
            }

            // Load saved provider
            var savedProvider = Settings.LoadMapProvider();
            if (savedProvider != null)
            {
                MapProvider = savedProvider;
            }

        }

        private void ExecuteNewRoute()
        {
            RoutePoints.Clear();
            StatusMessage = "New route created";
        }

        private void ExecuteOpenRoute()
        {
            // TODO: Implement open route functionality
            StatusMessage = "Opening route...";
        }

        private void ExecuteSaveRoute()
        {
            // TODO: Implement save route functionality
            StatusMessage = "Saving route...";
        }

        private void ExecuteExit()
        {
            Application.Current.Shutdown();
        }

        private void ExecuteFitToRoute()
        {
            RectLatLng? rectOfAllMarkers = mapControl.GetRectOfAllMarkers(null);
            if (rectOfAllMarkers.HasValue)
            {
                // Add 20% padding to the bounds
                var padding = new
                {
                    Lat = (rectOfAllMarkers.Value.Top - rectOfAllMarkers.Value.Bottom) * 0.1,
                    Lng = (rectOfAllMarkers.Value.Right - rectOfAllMarkers.Value.Left) * 0.1
                };

                RectLatLng paddedRect = rectOfAllMarkers.Value;
                paddedRect.Inflate(padding.Lat, padding.Lng);
                mapControl.SetZoomToFitRect(paddedRect);
            }
            // same as this mapControl.ZoomAndCenterMarkers(null);
            StatusMessage = "Fitting view to route...";
        }

        private PointLatLng? GetNearestPointOnRoad(PointLatLng point)
        {
            // Get the routing provider (OpenStreetMap in this case)
            var routingProvider = GMapProviders.OpenStreetMap as RoutingProvider;
            if (routingProvider == null) return null;

            // Get a route that passes through our point
            var route = routingProvider.GetRoute(
                point,
                point,
                false, false, (int)mapControl.Zoom);

            if (route == null || route.Points.Count == 0) return null;

            // Find the nearest point on the route
            var nearestPoint = route.Points
                .OrderBy(p => GMap.NET.MapProviders.OpenStreetMapProvider.Instance.Projection.GetDistance(p, point))
                .First();

            // Check if the nearest point is within the configured distance
            var distance = GMap.NET.MapProviders.OpenStreetMapProvider.Instance.Projection.GetDistance(nearestPoint, point);
            if (distance > Settings.LoadRoadSnapDistance() / 1000.0) // Convert meters to kilometers
            {
                return null;
            }

            return nearestPoint;
        }

        private void ExecuteAddPointAtLocation()
        {
            var clickedPoint = mapControl.LastRightClickPosition;
            var nearestRoadPoint = GetNearestPointOnRoad(clickedPoint);
            if (nearestRoadPoint == null)
            {
                StatusMessage = "No road found at clicked point";
                nearestRoadPoint = clickedPoint;
            } else {
                StatusMessage = "Point on road added from map";
            }

            var newPoint = new RoutePoint(
                mapControl,
                RoutePoints.Count + 1,
                nearestRoadPoint.Value
            );

            // Add both to collections
            RoutePoints.Add(newPoint);
            mapControl.Markers.Add(newPoint.Marker);
            
        }

        private void ExecuteDeletePoint()
        {
            if (SelectedPoint != null)
            {
                mapControl.Markers.Remove(SelectedPoint.Marker);
                RoutePoints.Remove(SelectedPoint);
                StatusMessage = "Point deleted";
                UpdateRoutePointIndices();
            }
        }

        private void ExecuteSettings()
        {
            var settingsWindow = new SettingsWindow
            {
                Owner = Application.Current.MainWindow
            };
            if (settingsWindow.ShowDialog() == true)
            {
                // Update center if settings were saved
                Center = new PointLatLng(Settings.LoadStartLatitude(), Settings.LoadStartLongitude());
                StatusMessage = "Settings saved";
            }
        }

        public void RoutePointsDataGrid_HandleDragStart(MouseButtonEventArgs e)
        {
            var row = ItemsControl.ContainerFromElement((DataGrid)e.Source, e.OriginalSource as DependencyObject) as DataGridRow;
            if (row != null)
            {
                draggedItem = row.Item as RoutePoint;
                if (draggedItem != null)
                {
                    DragDrop.DoDragDrop(row, draggedItem, DragDropEffects.Move);
                }
            }
        }

        public void RoutePointsDataGrid_HandleDrop(DragEventArgs e)
        {
            var row = ItemsControl.ContainerFromElement((DataGrid)e.Source, e.OriginalSource as DependencyObject) as DataGridRow;
            if (row != null && draggedItem != null)
            {
                var targetItem = row.Item as RoutePoint;
                if (targetItem != null && draggedItem != targetItem)
                {
                    var oldIndex = RoutePoints.IndexOf(draggedItem);
                    var newIndex = RoutePoints.IndexOf(targetItem);

                    RoutePoints.RemoveAt(oldIndex);
                    RoutePoints.Insert(newIndex, draggedItem);

                    UpdateRoutePointIndices();
                }
            }
            draggedItem = null;
        }

        public void RoutePointsDataGrid_HandleDragOver(DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        private void UpdateRoutePointIndices()
        {
            // Update indices
            for (int i = 0; i < RoutePoints.Count; i++)
            {
                RoutePoints[i].Index = i + 1;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 
