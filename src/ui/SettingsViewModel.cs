using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace nl.siwoc.RouteManager.ui
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private int roadSnapDistance;
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

        public SettingsViewModel()
        {
            StartLatitude = Settings.LoadStartLatitude();
            StartLongitude = Settings.LoadStartLongitude();
            RoadSnapDistance = Settings.LoadRoadSnapDistance();
        }

        public void SaveSettings()
        {
            Settings.SaveStartLatitude(StartLatitude);
            Settings.SaveStartLongitude(StartLongitude);
            Settings.SaveRoadSnapDistance(RoadSnapDistance);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 