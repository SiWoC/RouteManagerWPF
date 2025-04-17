using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace nl.siwoc.RouteManager
{
    public class RoutePoint : GMapMarker, INotifyPropertyChanged
    {
        private string name;
        private string notes;
        private int index;

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

        public int Index
        {
            get => index;
            set
            {
                if (index != value)
                {
                    index = value;
                    OnPropertyChanged();
                }
            }
        }

        public RoutePoint(string name, double latitude, double longitude, int index, string notes = "")
            : base(new PointLatLng(latitude, longitude))
        {
            Name = name;
            Notes = notes;
            Index = index;
            
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 