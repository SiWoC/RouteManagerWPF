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

        public MainViewModel(MapControlWrapper mapControl)
        {
            this.mapControl = mapControl;
            this.mapControl.ShowCenter = false;
            center = new PointLatLng(46.538615, 10.501385);

            // Initialize commands
            NewRouteCommand = new RelayCommand(ExecuteNewRoute);
            OpenRouteCommand = new RelayCommand(ExecuteOpenRoute);
            SaveRouteCommand = new RelayCommand(ExecuteSaveRoute);
            ExitCommand = new RelayCommand(ExecuteExit);
            FitToRouteCommand = new RelayCommand(ExecuteFitToRoute);
            AddPointAtLocationCommand = new RelayCommand(ExecuteAddPointAtLocation);
            DeletePointCommand = new RelayCommand(ExecuteDeletePoint);

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

            // Handle map contextmenu to add points
            mapControl.AddRoutePointRequested += (s, point) =>
            {
                var newPoint = new RoutePoint($"Point {RoutePoints.Count + 1}", point.Lat, point.Lng, RoutePoints.Count + 1);
                RoutePoints.Add(newPoint);
                StatusMessage = "Point added from map";
            };
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

        private void ExecuteAddPointAtLocation()
        {
            var newPoint = new RoutePoint(
                $"Point {RoutePoints.Count + 1}",
                mapControl.LastRightClickPosition.Lat,
                mapControl.LastRightClickPosition.Lng,
                RoutePoints.Count + 1
            );

            // Create marker with CustomMarkerDemo
            var m = new GMapMarker(mapControl.LastRightClickPosition);
            {
                Placemark? p = null;
                if (true)
                {
                    GeoCoderStatusCode status;
                    var plret = GMapProviders.GoogleMap.GetPlacemark(mapControl.LastRightClickPosition, out status);
                    if (status == GeoCoderStatusCode.OK && plret != null)
                    {
                        p = plret;
                    }
                }

                string toolTipText;
                if (p != null)
                {
                    toolTipText = p.Value.Address;
                }
                else
                {
                    toolTipText = mapControl.LastRightClickPosition.ToString();
                }

                m.Shape = new CustomMarkerDemo(mapControl, m, toolTipText, newPoint.Index);
            }

            // Add both to collections
            RoutePoints.Add(newPoint);
            mapControl.Markers.Add(newPoint);
            mapControl.Markers.Add(m);
            
            StatusMessage = "Point added from map";
        }

        private void ExecuteDeletePoint()
        {
            if (SelectedPoint != null)
            {
                mapControl.Markers.Remove(SelectedPoint);
                RoutePoints.Remove(SelectedPoint);
                StatusMessage = "Point deleted";
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 
