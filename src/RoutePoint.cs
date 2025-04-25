using GMap.NET;
using GMap.NET.WindowsPresentation;
using nl.siwoc.RouteManager.ui;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace nl.siwoc.RouteManager
{
    public class RoutePoint : INotifyPropertyChanged
    {
        private int index;
        private MapControlWrapper mapControl;
        private string name;
        private string notes;
        private PointLatLng position;
        public GMapMarker Marker { get; private set; }

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

        public RoutePoint(MapControlWrapper mapControl, int index, PointLatLng position)
        {
            this.mapControl = mapControl;
            this.position = position;
            Index = index;
            Name = $"Point {Index}";

            Marker = new GMapMarker(Position);
            Marker.Shape = new FlagMarker(mapControl, Marker, Name, Index);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 