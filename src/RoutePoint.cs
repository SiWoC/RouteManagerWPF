using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace nl.siwoc.RouteManager
{
    public class RoutePoint : INotifyPropertyChanged
    {
        private string name;
        private double latitude;
        private double longitude;
        private string notes;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Latitude
        {
            get => latitude;
            set
            {
                if (latitude != value)
                {
                    latitude = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Longitude
        {
            get => longitude;
            set
            {
                if (longitude != value)
                {
                    longitude = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Notes
        {
            get => notes;
            set
            {
                if (notes != value)
                {
                    notes = value;
                    OnPropertyChanged();
                }
            }
        }

        public RoutePoint(string name, double latitude, double longitude, string notes = "")
        {
            Name = name;
            Latitude = latitude;
            Longitude = longitude;
            Notes = notes;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 