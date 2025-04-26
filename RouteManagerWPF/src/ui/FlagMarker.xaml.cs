using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using GMap.NET.WindowsPresentation;

namespace nl.siwoc.RouteManager.ui
{
    public partial class FlagMarker
    {
        private Popup _popup;
        private Label _label;
        private GMapMarker _marker;
        private MapControlWrapper mapControl;

        public FlagMarker(MapControlWrapper mapControl, GMapMarker marker, string title, int index)
        {
            InitializeComponent();

            this.mapControl = mapControl;
            _marker = marker;
            FlagText.Text = index.ToString();
            if (index == 1) {
                FlagCloth.Fill = Brushes.Green;
            }

            // Set the marker offset to position the flag above its point
            _marker.Offset = new Point(0, -Height);

            _popup = new Popup();
            _label = new Label();

            MouseEnter += MarkerControl_MouseEnter;
            MouseLeave += MarkerControl_MouseLeave;
            MouseMove += FlagMarker_MouseMove;
            MouseLeftButtonUp += FlagMarker_MouseLeftButtonUp;
            MouseLeftButtonDown += FlagMarker_MouseLeftButtonDown;

            _popup.Placement = PlacementMode.Mouse;
            {
                _label.Background = Brushes.Blue;
                _label.Foreground = Brushes.White;
                _label.BorderBrush = Brushes.WhiteSmoke;
                _label.BorderThickness = new Thickness(2);
                _label.Padding = new Thickness(5);
                _label.FontSize = 22;
                _label.Content = title;
            }
            _popup.Child = _label;
        }

        void FlagMarker_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
            {
                var p = e.GetPosition(mapControl);
                _marker.Position = mapControl.FromLocalToLatLng((int)p.X, (int)p.Y);
            }
        }

        void FlagMarker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseCaptured)
            {
                Mouse.Capture(this);
            }
        }

        void FlagMarker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                Mouse.Capture(null);
            }
        }

        void MarkerControl_MouseLeave(object sender, MouseEventArgs e)
        {
            _marker.ZIndex -= 10000;
            _popup.IsOpen = false;
        }

        void MarkerControl_MouseEnter(object sender, MouseEventArgs e)
        {
            _marker.ZIndex += 10000;
            _popup.IsOpen = true;
        }
    }
}
