using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System.Windows.Input;

namespace nl.siwoc.RouteManager.ui
{
    public class MapControlWrapper : GMapControl
    {
        public PointLatLng LastRightClickPosition { get; private set; }

        public MapControlWrapper()
        {
            // Configure GMap.NET
            GMapProvider.WebProxy = null;
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            GMaps.Instance.UseDirectionsCache = true;
            GMaps.Instance.UseGeocoderCache = true;
            GMaps.Instance.UseMemoryCache = true;
            GMaps.Instance.UsePlacemarkCache = true;
            GMaps.Instance.UseRouteCache = true;
            GMaps.Instance.UseUrlCache = true;

            // Initialize map settings
            ShowTileGridLines = false;
            MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
            MinZoom = 2;
            MaxZoom = 20;
            DragButton = MouseButton.Left;

            MouseRightButtonDown += (s, e) =>
            {
                LastRightClickPosition = FromLocalToLatLng((int)e.GetPosition(this).X, (int)e.GetPosition(this).Y);
                System.Diagnostics.Debug.WriteLine($"MousePosition saved: {LastRightClickPosition.ToString() ?? "null"}");
                e.Handled = true;
            };

        }


    }
} 