using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;

namespace nl.siwoc.RouteManager
{
    public class MapControlWrapper : GMapControl
    {
        public event EventHandler<PointLatLng> AddRoutePointRequested;
        public PointLatLng LastRightClickPosition { get; private set; }

        public MapControlWrapper()
        {
            // Configure GMap.NET
            GMapProvider.WebProxy = null;
            GMaps.Instance.Mode = AccessMode.ServerAndCache;

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

            System.Diagnostics.Debug.WriteLine($"MapControlWrapper initialized with provider: {base.MapProvider?.Name ?? "null"}");
        }

        /// <summary>
        /// Gets a curated list of map providers
        /// </summary>
        /// <returns>List of selected GMapProviders</returns>
        public static List<GMapProvider> GetAllMapProviders()
        {
            return new List<GMapProvider>
            {
                OpenStreetMapProvider.Instance,
                BingMapProvider.Instance,
                BingSatelliteMapProvider.Instance,
                BingHybridMapProvider.Instance,
                BingOSMapProvider.Instance,
                GoogleMapProvider.Instance,
                GoogleSatelliteMapProvider.Instance,
                GoogleHybridMapProvider.Instance,
                GoogleTerrainMapProvider.Instance
            };
        }
    }
} 