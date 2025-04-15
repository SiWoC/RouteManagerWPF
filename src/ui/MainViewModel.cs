using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using GMap.NET;
using GMap.NET.MapProviders;

namespace nl.siwoc.RouteManager
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string statusMessage = "Ready";
        private int zoomLevel = 15;
        private ObservableCollection<RoutePoint> routePoints = new ObservableCollection<RoutePoint>();
        private PointLatLng center = new PointLatLng(52.3676, 4.9041); // Default to Amsterdam
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

        public int ZoomLevel
        {
            get => zoomLevel;
            set
            {
                if (zoomLevel != value)
                {
                    zoomLevel = value;
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
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand FitToRouteCommand { get; }
        public ICommand AddPointCommand { get; }
        public ICommand DeletePointCommand { get; }

        public MainViewModel(MapControlWrapper mapControl)
        {
            this.mapControl = mapControl;

            // Initialize commands
            NewRouteCommand = new RelayCommand(ExecuteNewRoute);
            OpenRouteCommand = new RelayCommand(ExecuteOpenRoute);
            SaveRouteCommand = new RelayCommand(ExecuteSaveRoute);
            ExitCommand = new RelayCommand(ExecuteExit);
            ZoomInCommand = new RelayCommand(ExecuteZoomIn);
            ZoomOutCommand = new RelayCommand(ExecuteZoomOut);
            FitToRouteCommand = new RelayCommand(ExecuteFitToRoute);
            AddPointCommand = new RelayCommand(ExecuteAddPoint);
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

        private void ExecuteZoomIn()
        {
            if (ZoomLevel < 20)
            {
                ZoomLevel++;
                StatusMessage = $"Zoom level: {ZoomLevel}";
            }
        }

        private void ExecuteZoomOut()
        {
            if (ZoomLevel > 0)
            {
                ZoomLevel--;
                StatusMessage = $"Zoom level: {ZoomLevel}";
            }
        }

        private void ExecuteFitToRoute()
        {
            // TODO: Implement fit to route functionality
            StatusMessage = "Fitting view to route...";
        }

        private void ExecuteAddPoint()
        {
            var newPoint = new RoutePoint("New Point", 0, 0);
            RoutePoints.Add(newPoint);
            StatusMessage = "Point added";
        }       

        private void ExecuteDeletePoint()
        {
            if (SelectedPoint != null)
            {
                RoutePoints.Remove(SelectedPoint);
                StatusMessage = "Point deleted";
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute();
        }

        public void Execute(object parameter)
        {
            execute();
        }
    }
} 