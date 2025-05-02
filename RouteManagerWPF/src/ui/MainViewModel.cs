using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using GMap.NET;
using GMap.NET.MapProviders;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;
using System.Collections.Specialized;
using System.Linq;
using nl.siwoc.RouteManager.fileFormats;
using System.Windows.Threading;

namespace nl.siwoc.RouteManager.ui
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string statusMessage = "Ready";
        private ObservableCollection<RoutePoint> routePoints = new ObservableCollection<RoutePoint>();
        private PointLatLng center;
        private GMapProvider mapProvider;
        private bool showTrafficLayer;
        private readonly MapControlWrapper mapControl;
        private RoutePoint selectedPoint;
        private RoutePoint draggedItem;
        private readonly RoutePolyline routePolyline;
        private readonly DispatcherTimer updateRouteTimer;
        private bool routeUpdatePending;
        private string routeName = "New Route";
        private double routeDistance;
        private string routeDurationDisplay;
        private bool isDirty;
        private string currentFileName;

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
                    GoogleMapProvider googleMapProvider = value as GoogleMapProvider;
                    if (googleMapProvider != null && showTrafficLayer)
                    {
                        googleMapProvider.Version = "m,traffic";
                        mapProvider = googleMapProvider;
                    }
                    else
                    {
                        mapProvider = value;
                    }
                    OnPropertyChanged();
                    
                    // Update the MapControlWrapper
                    mapControl.MapProvider = mapProvider;
                    System.Diagnostics.Debug.WriteLine($"MapControlWrapper MapProvider set to: {value?.Name ?? "null"}");

                    // Save the provider
                    Settings.SaveMapProvider(value);
                }
            }
        }

        public bool ShowTrafficLayer
        {
            get => showTrafficLayer;
            set
            {
                if (showTrafficLayer != value)
                {
                    showTrafficLayer = value;
                    OnPropertyChanged();
                    Settings.SaveShowTrafficLayer(value);
                    UpdateTrafficLayer();
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
                    if (routePoints != null)
                    {
                        routePoints.CollectionChanged -= RoutePoints_CollectionChanged;
                    }
                    routePoints = value;
                    if (routePoints != null)
                    {
                        routePoints.CollectionChanged += RoutePoints_CollectionChanged;
                    }
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
                    if (Application.Current.MainWindow is MainWindow mainWindow)
                    {
                        mainWindow.ScrollToSelectedPoint();
                    }
                }
            }
        }

        public string RouteName
        {
            get => routeName;
            set
            {
                if (routeName != value)
                {
                    routeName = value;
                    IsDirty = true;
                    OnPropertyChanged();
                }
            }
        }

        public double RouteDistance
        {
            get => routeDistance;
            set
            {
                if (routeDistance != value)
                {
                    routeDistance = value;
                    OnPropertyChanged();
                }
            }
        }

        public string RouteDuration
        {
            get => routeDurationDisplay;
            private set
            {
                if (routeDurationDisplay != value)
                {
                    routeDurationDisplay = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsDirty
        {
            get => isDirty;
            set
            {
                if (isDirty != value)
                {
                    isDirty = value;
                    OnPropertyChanged();
                    UpdateWindowTitle();
                }
            }
        }

        public string CurrentFileName
        {
            get => currentFileName;
            set
            {
                if (currentFileName != value)
                {
                    currentFileName = value;
                    OnPropertyChanged();
                    UpdateWindowTitle();
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


        public MainViewModel(MapControlWrapper mapControl)
        {
            this.mapControl = mapControl;
            this.mapControl.ShowCenter = false;
            center = new PointLatLng(Settings.LoadStartLatitude(), Settings.LoadStartLongitude());

            routePoints.CollectionChanged += RoutePoints_CollectionChanged;

            // Initialize debounce timer
            updateRouteTimer = new DispatcherTimer();
            updateRouteTimer.Interval = TimeSpan.FromMilliseconds(500);
            updateRouteTimer.Tick += (s, e) => {
                if (routeUpdatePending)
                {
                    routeUpdatePending = false;
                    var (distance, duration) = routePolyline.UpdateRoute(RoutePoints);
                    RouteDistance = distance;
                    RouteDuration = Utils.ConvertRouteDuration(duration);
                }
            };

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
            foreach (var provider in Settings.GetAllMapProviders())
            {
                MapProviders.Add(provider);
            }

            showTrafficLayer = Settings.LoadShowTrafficLayer();
            MapProvider = Settings.LoadMapProvider();

            // Initialize route polyline after provider is set
            routePolyline = new RoutePolyline(mapControl);
        }

        internal void ScheduleRouteUpdate()
        {
            routeUpdatePending = true;
            updateRouteTimer.Stop();
            updateRouteTimer.Start();
        }

        private void RoutePoints_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ScheduleRouteUpdate();
            IsDirty = true;

            if (e.NewItems != null)
            {
                foreach (RoutePoint point in e.NewItems)
                {
                    if (point.MapControl != null)
                    {
                        point.PropertyChanged += RoutePoint_PropertyChanged;
                        SetupFlagEvents(point);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (RoutePoint point in e.OldItems)
                {
                    point.PropertyChanged -= RoutePoint_PropertyChanged;
                }
            }
        }

        private void RoutePoint_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Don't mark as dirty for calculated fields
            if (e.PropertyName != nameof(RoutePoint.DisplayText) && 
                e.PropertyName != nameof(RoutePoint.CumulativeDistance) &&
                e.PropertyName != nameof(RoutePoint.CumulativeDuration))
            {
                IsDirty = true;
            }
        }

        private void SetupFlagEvents(RoutePoint point)
        {
            var flagMarker = (FlagMarker)point.Marker.Shape;
            flagMarker.ClickCommand = new RelayCommand(() => SelectedPoint = point);
            flagMarker.PositionChangedCommand = new RelayCommand<PointLatLng>(newPos => {
                if (point.Position != newPos)
                {
                    point.Position = newPos;
                    ScheduleRouteUpdate();
                }
            });
            flagMarker.DropCommand = new RelayCommand<PointLatLng>(newPos => {
                point.Position = GetNearestPointOnRoad(newPos);
                EnrichRoutePoint(point);
                ScheduleRouteUpdate();
            });
        }

        internal bool ConfirmUnsavedChanges()
        {
            if (!IsDirty || routePoints.Count == 0) return true;

            var result = MessageBox.Show("The route has changed, do you want to save?", "Unsaved Changes", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                ExecuteSaveRoute();
                return !IsDirty; // Return false if user cancelled save
            } else if (result == MessageBoxResult.No) {
                isDirty = false;
            }
            return result != MessageBoxResult.Cancel;
        }

        private void ExecuteNewRoute()
        {
            if (!ConfirmUnsavedChanges()) return;

            mapControl.Markers.Clear();
            RoutePoints.Clear();
            routePolyline.Clear(); // will not be cleared automatically by recreation as with OpenRoute
            StatusMessage = "New route created";
            RouteName = "New Route";
            CurrentFileName = null;
            IsDirty = false;
        }

        private void ExecuteOpenRoute()
        {
            if (!ConfirmUnsavedChanges()) return;

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "All supported files|*.trp;*.gpx|CoPilot TRP files|*.trp|GPX files|*.gpx|All files|*.*",
                DefaultExt = ".trp"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Temporarily disable CollectionChanged events
                    routePoints.CollectionChanged -= RoutePoints_CollectionChanged;

                    IFileParser parser;
                    if (dialog.FileName.EndsWith(".gpx", StringComparison.OrdinalIgnoreCase))
                    {
                        parser = new GpxFileParser();
                    }
                    else
                    {
                        parser = new CoPilotTrpFileParser();
                    }
                    var (points, routeName) = parser.Read(dialog.FileName);
                    
                    // Clear all markers and points
                    mapControl.Markers.Clear();
                    RoutePoints.Clear();
                    
                    // Add new points to existing collection
                    foreach (var point in points)
                    {
                        point.MapControl = mapControl;
                        point.PropertyChanged += RoutePoint_PropertyChanged;
                        SetupFlagEvents(point);
                        RoutePoints.Add(point);
                    }
                    UpdateRoutePointIndices();
                    RouteName = routeName;
                    CurrentFileName = dialog.FileName;
                    IsDirty = false;
                    ExecuteFitToRoute();
                    SelectedPoint = RoutePoints[0];
                    
                    StatusMessage = "Route loaded successfully";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error loading route: {ex.Message}";
                }
                finally
                {
                    // Re-enable CollectionChanged events
                    routePoints.CollectionChanged += RoutePoints_CollectionChanged;
                    // Trigger one final update
                    ScheduleRouteUpdate();
                }
            }
        }

        private void ExecuteSaveRoute()
        {
            if (RoutePoints.Count == 0)
            {
                StatusMessage = "No route points to save";
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "All supported files|*.trp;*.gpx|CoPilot TRP files|*.trp|GPX files|*.gpx|All files|*.*",
                DefaultExt = ".trp",
                FileName = CurrentFileName
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    IFileParser parser;
                    if (dialog.FileName.EndsWith(".gpx", StringComparison.OrdinalIgnoreCase))
                    {
                        parser = new GpxFileParser();
                    }
                    else
                    {
                        parser = new CoPilotTrpFileParser();
                    }
                    parser.Write(dialog.FileName, RoutePoints.ToList(), RouteName);
                    CurrentFileName = dialog.FileName;
                    IsDirty = false;
                    StatusMessage = "Route saved successfully";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error saving route: {ex.Message}";
                }
            }
        }

        private void ExecuteExit()
        {
            if (!ConfirmUnsavedChanges()) return;
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

        private PointLatLng GetNearestPointOnRoad(PointLatLng point)
        {
            // Get a route that passes through our point
            var route = Settings.LoadRoutingProvider().GetRoute(
                point,
                point,
                false, false, (int)mapControl.Zoom);

            if (route == null || route.Points.Count == 0) return point;

            // Find the nearest point on the route
            var nearestPoint = route.Points
                .OrderBy(p => mapProvider.Projection.GetDistance(p, point))
                .First();

            // Check if the nearest point is within the configured distance
            var distance = mapProvider.Projection.GetDistance(nearestPoint, point);
            if (distance > Settings.LoadRoadSnapDistance() / 1000.0) // Convert meters to kilometers
            {
                StatusMessage = "No road found at clicked point";
                return point;
            }
            else
            {
                StatusMessage = "Point on road added from map";
                return nearestPoint;
            }
        }

        private void ExecuteAddPointAtLocation()
        {
            var newPoint = new RoutePoint(1, GetNearestPointOnRoad(mapControl.LastRightClickPosition));
            EnrichRoutePoint(newPoint);
            newPoint.MapControl = mapControl;
            
            if (SelectedPoint != null)
            {
                RoutePoints.Insert(RoutePoints.IndexOf(SelectedPoint) + 1, newPoint);
            }
            else
            {
                RoutePoints.Add(newPoint);
            }
            SelectedPoint = newPoint;
            UpdateRoutePointIndices();
        }

        private void EnrichRoutePoint(RoutePoint routePoint)
        {
            Placemark? p = GetPlacemark(routePoint.Position);
            if (p != null)
            {
                routePoint.Country = p.Value.CountryName;
                routePoint.City = p.Value.LocalityName ?? p.Value.DistrictName ?? p.Value.SubAdministrativeAreaName;
                routePoint.Zip = p.Value.PostalCodeNumber;
                routePoint.Address = string.Join(" ", new[] { p.Value.ThoroughfareName, p.Value.StreetNumber }.Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        private Placemark? GetPlacemark(PointLatLng point)
        {
            GeoCoderStatusCode status;
            Placemark? plret;
            if (!string.IsNullOrEmpty(Settings.LoadGoogleApiKey()))
            {
                plret = GMapProviders.GoogleMap.GetPlacemark(point, out status);
            }
            else
            {
                plret = GMapProviders.OpenStreetMap.GetPlacemark(point, out status);
            }
            if (status == GeoCoderStatusCode.OK && plret != null)
            {
                return plret;
            }
            return null;
        }

        private void ExecuteDeletePoint()
        {
            if (SelectedPoint != null)
            {
                SelectedPoint.MapControl = null;  // This will remove the marker
                RoutePoints.Remove(SelectedPoint);
                StatusMessage = "Point deleted";
                UpdateRoutePointIndices();
            }
        }

        private void ExecuteSettings()
        {
            var settingsWindow = new SettingsWindow
            {
                Owner = Application.Current.MainWindow,
                DataContext = new SettingsViewModel(this)
            };
            if (settingsWindow.ShowDialog() == true)
            {
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
                RoutePoints[i].IsFinish = (i == RoutePoints.Count - 1);
                RoutePoints[i].UpdateStyle();
            }
        }

        private void UpdateWindowTitle()
        {
            var title = "Route Manager";
            if (!string.IsNullOrEmpty(CurrentFileName))
            {
                title += $" - {System.IO.Path.GetFileName(CurrentFileName)}";
            }
            if (!string.IsNullOrEmpty(RouteName))
            {
                title += $" - {RouteName}";
            }
            if (IsDirty)
            {
                title += " *";
            }
            Application.Current.MainWindow.Title = title;
        }

        private void UpdateTrafficLayer()
        {
            if (mapProvider is GoogleMapProvider googleMapProvider)
            {
                googleMapProvider.Version = showTrafficLayer ? "m,traffic" : "m";
                // Clear Google Maps cache to ensure traffic layer changes take effect
                GMaps.Instance.MemoryCache.Clear();
                GMaps.Instance.PrimaryCache.DeleteOlderThan(DateTime.Now, null);
                mapControl.MapProvider = mapProvider; // Force refresh
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 
