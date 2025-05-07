using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using GMap.NET.WindowsPresentation;
using CommunityToolkit.Mvvm.Input;
using GMap.NET;

namespace nl.siwoc.RouteManager.ui
{
    public partial class FlagMarker
    {
        public enum FlagStyle { Start, Stop, RoutePoint, Finish, Selected }

        private GMapMarker _marker;
        private MapControlWrapper mapControl;
        private Point _dragStartPoint;

        public ICommand ClickCommand { get; set; }
        public ICommand PositionChangedCommand { get; set; }
        public ICommand DropCommand { get; set; }

        public int Index
        {
            set
            {
                FlagText.Text = value.ToString();
            }
        }

        public void SetStyle(FlagStyle style) 
        {
            if (style == FlagStyle.Selected)
            {
                FlagCloth.Style = (Style)Resources["SelectedCloth"];
                Pole.Style = (Style)Resources["SelectedPole"];
                _marker.ZIndex = 200;
            }
            else
            {
                FlagCloth.Style = (Style)Resources[style.ToString()];
                Pole.Style = (Style)Resources[style.ToString()];
                _marker.ZIndex = 100;
            }
        }

        public FlagMarker(MapControlWrapper mapControl, GMapMarker marker, int index)
        {
            InitializeComponent();

            this.mapControl = mapControl;
            _marker = marker;
            _marker.ZIndex = 100;
            Index = index;

            // Set the marker offset to position the flag above its point
            _marker.Offset = new Point(0, -Height);

            MouseMove += FlagMarker_MouseMove;
            MouseLeftButtonUp += FlagMarker_MouseLeftButtonUp;
            MouseLeftButtonDown += FlagMarker_MouseLeftButtonDown;
        }

        void FlagMarker_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
            {
                var currentPoint = e.GetPosition(this);
                var diff = currentPoint - _dragStartPoint;
                
                if (Math.Abs(diff.X) >= 5 || Math.Abs(diff.Y) >= 5) // Only move if we've actually dragged
                {
                    var p = e.GetPosition(mapControl);
                    var newPosition = mapControl.FromLocalToLatLng((int)p.X, (int)p.Y);
                    PositionChangedCommand?.Execute(newPosition);
                    e.Handled = true;
                }
            }
        }

        void FlagMarker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseCaptured)
            {
                _dragStartPoint = e.GetPosition(this);
                Mouse.Capture(this);
                e.Handled = true;
            }
        }

        void FlagMarker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                Mouse.Capture(null);
                var currentPoint = e.GetPosition(this);
                var diff = currentPoint - _dragStartPoint;
                
                if (Math.Abs(diff.X) < 5 && Math.Abs(diff.Y) < 5) // If moved less than 5 pixels, treat as click
                {
                    ClickCommand?.Execute(null);
                }
                else
                {
                    var p = e.GetPosition(mapControl);
                    var newPosition = mapControl.FromLocalToLatLng((int)p.X, (int)p.Y);
                    DropCommand?.Execute(newPosition);
                    ClickCommand?.Execute(null);
                }
                e.Handled = true;
            }
        }
    }
}
