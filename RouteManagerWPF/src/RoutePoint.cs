using GMap.NET;
using GMap.NET.WindowsPresentation;
using nl.siwoc.RouteManager.ui;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace nl.siwoc.RouteManager
{
    public class RoutePoint : INotifyPropertyChanged
    {
        private MapControlWrapper mapControl;

        private int index;
        private PointLatLng position;
        private string name;
        private string country;
        private string city;
        private string zip;
        private string address;
        private bool isStop;
        public GMapMarker Marker { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public MapControlWrapper MapControl
        {
            get => mapControl;
            set
            {
                if (mapControl != value)
                {
                    // Remove old marker if exists
                    if (mapControl != null && Marker != null)
                    {
                        mapControl.Markers.Remove(Marker);
                    }

                    mapControl = value;

                    // Create new marker if we have a map control
                    if (mapControl != null)
                    {
                        Marker = new GMapMarker(Position);
                        Marker.Shape = new FlagMarker(mapControl, Marker, Name, Index);
                        mapControl.Markers.Add(Marker);
                    }
                }
            }
        }

        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayText));
                }
            }
        }

        public string Country
        {
            get => country;
            set
            {
                if (country != value)
                {
                    country = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayText));
                }
            }
        }

        public string City
        {
            get => city;
            set
            {
                if (city != value)
                {
                    city = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayText));
                }
            }
        }

        public string Zip
        {
            get => zip;
            set
            {
                if (zip != value)
                {
                    zip = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayText));
                }
            }
        }

        public string Address
        {
            get => address;
            set
            {
                if (address != value)
                {
                    address = value;
                    if (string.IsNullOrEmpty(name))
                    {
                        name = address;
                    }
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayText));
                }
            }
        }

        public bool IsStop
        {
            get => isStop;
            set
            {
                if (isStop != value)
                {
                    isStop = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DisplayText));
                }
            }
        }

        public string DisplayText => $"{Name} ({Position.Lat:F5}, {Position.Lng:F5})";

        public int Index
        {
            get => index;
            set
            {
                if (index != value)
                {
                    index = value;
                    if (Marker != null)
                    {
                        (Marker.Shape as FlagMarker).FlagText.Text = value.ToString();
                    }
                    OnPropertyChanged();
                }
            }
        }

        public PointLatLng Position
        {
            get => position;
            set
            {
                if (position != value)
                {
                    position = value;
                    if (Marker != null)
                    {
                        Marker.Position = value;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public RoutePoint(int index, PointLatLng position)
        {
            this.position = position;
            Index = index;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 