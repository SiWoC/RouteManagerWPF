using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using GMap.NET.MapProviders;
using System.Linq;
using GMap.NET;

namespace nl.siwoc.RouteManager.ui
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private int roadSnapDistance;
        private string googleApiKey;
        private RoutingProvider selectedRoutingProvider;
        private bool routeProviderChanged = false;
        private readonly MainViewModel mainViewModel;
        public ObservableCollection<RoutingProvider> AvailableRoutingProviders { get; } = new ObservableCollection<RoutingProvider>();

        public int RoadSnapDistance
        {
            get => roadSnapDistance;
            set
            {
                if (roadSnapDistance != value)
                {
                    roadSnapDistance = value;
                    OnPropertyChanged();
                }
            }
        }

        private double startLatitude;
        public double StartLatitude
        {
            get => startLatitude;
            set
            {
                if (startLatitude != value)
                {
                    startLatitude = value;
                    OnPropertyChanged();
                }
            }
        }

        private double startLongitude;
        public double StartLongitude
        {
            get => startLongitude;
            set
            {
                if (startLongitude != value)
                {
                    startLongitude = value;
                    OnPropertyChanged();
                }
            }
        }

        public RoutingProvider SelectedRoutingProvider
        {
            get => selectedRoutingProvider;
            set
            {
                if (selectedRoutingProvider != value)
                {
                    selectedRoutingProvider = value;
                    OnPropertyChanged();
                    routeProviderChanged = true;
                }
            }
        }

        public string GoogleApiKey
        {
            get => googleApiKey;
            set
            {
                if (googleApiKey != value)
                {
                    googleApiKey = value;
                    OnPropertyChanged();
                    UpdateAvailableRoutingProviders();
                }
            }
        }

        private void UpdateAvailableRoutingProviders()
        {
            AvailableRoutingProviders.Clear();
            AvailableRoutingProviders.Add(OpenStreetMapProvider.Instance);
            if (!string.IsNullOrEmpty(GoogleApiKey))
            {
                AvailableRoutingProviders.Add(GoogleMapProvider.Instance);
            }
        }

        public SettingsViewModel(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
            StartLatitude = Settings.GetStartLatitude();
            StartLongitude = Settings.GetStartLongitude();
            RoadSnapDistance = Settings.GetRoadSnapDistance();
            GoogleApiKey = Settings.LoadGoogleApiKey();
            UpdateAvailableRoutingProviders();
            SelectedRoutingProvider = AvailableRoutingProviders.FirstOrDefault(p => p.GetType().Name == Settings.GetRoutingProviderName()) 
                ?? AvailableRoutingProviders[0];
        }

        public void SaveSettings()
        {
            Settings.SaveStartLatitude(StartLatitude);
            Settings.SaveStartLongitude(StartLongitude);
            Settings.SaveRoadSnapDistance(RoadSnapDistance);
            Settings.SaveGoogleApiKey(GoogleApiKey);
            Settings.SaveRoutingProvider(SelectedRoutingProvider.GetType().Name);
            if (routeProviderChanged)
            {
                mainViewModel?.ScheduleRouteUpdate();
                routeProviderChanged = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 