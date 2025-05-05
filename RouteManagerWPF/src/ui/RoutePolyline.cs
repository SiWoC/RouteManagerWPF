using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Globalization;
using System.Windows;

namespace nl.siwoc.RouteManager.ui
{
    public class RoutePolyline
    {
        private readonly MapControlWrapper mapControl;
        private GMapRoute gmapRoute;

        public RoutePolyline(MapControlWrapper mapControl)
        {
            this.mapControl = mapControl;
        }

        public async Task<(double distance, double duration)> UpdateRoute(IEnumerable<RoutePoint> points)
        {
            double totalDistance = 0;
            double totalDuration = 0;
            var pointList = points.ToList();
            if (pointList.Count < 2)
            {
                if (gmapRoute != null)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() => {
                        mapControl.Markers.Remove(gmapRoute);
                    });
                    gmapRoute = null;
                }
                return (totalDistance, totalDuration);
            }

            // accumulates all polyline points from all route segments
            var allPoints = new List<PointLatLng>();
            for (int i = 0; i < pointList.Count - 1; i++)
            {
                var start = pointList[i].Position;
                var end = pointList[i + 1].Position;
                
                var zoom = await Application.Current.Dispatcher.InvokeAsync(() => (int)mapControl.Zoom);
                MapRoute mapRoute = await Task.Run(() => Settings.LoadRoutingProvider().GetRoute(start, end, false, false, zoom));
                if (mapRoute != null)
                {
                    allPoints.AddRange(mapRoute.Points);
                    totalDistance += mapRoute.Distance;
                    totalDuration += ParseDuration(mapRoute.Duration);
                    pointList[i + 1].CumulativeDistance = totalDistance;
                    pointList[i + 1].CumulativeDuration = Utils.ConvertRouteDuration(totalDuration);
                }
            }

            if (allPoints.Count > 0)
            {
                await Application.Current.Dispatcher.InvokeAsync(() => {
                    if (gmapRoute != null)
                    {
                        mapControl.Markers.Remove(gmapRoute);
                    }
                    gmapRoute = new GMapRoute(allPoints);
                    gmapRoute.Shape = new Path
                    {
                        Stroke = Brushes.Blue,
                        StrokeThickness = 4,
                        Opacity = 0.7
                    };
                    mapControl.Markers.Add(gmapRoute);
                });
            }

            return (totalDistance, totalDuration);
        }

        internal double ParseDuration(string duration)
        {
            if (string.IsNullOrEmpty(duration)) return 0;

            var provider = Settings.LoadRoutingProviderName();
            if (provider == Settings.GoogleMapProviderName)
            {
                // Google format: "2 hours 23 mins"
                double totalSeconds = 0;
                var parts = duration.Split(' ');
                for (int i = 0; i < parts.Length; i += 2)
                {
                    if (i + 1 >= parts.Length) break;
                    if (double.TryParse(parts[i], out double value))
                    {
                        if (parts[i + 1].StartsWith("hour")) totalSeconds += value * 3600;
                        else if (parts[i + 1].StartsWith("min")) totalSeconds += value * 60;
                    }
                }
                return totalSeconds;
            }
            else if (provider == Settings.OpenStreetMapProviderName)
            {
                // OSM format: seconds with decimal comma
                if (double.TryParse(duration.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double seconds))
                {
                    return seconds;
                }
            }

            return 0;
        }

        public void Clear()
        {
            if (gmapRoute != null)
            {
                mapControl.Markers.Remove(gmapRoute);
                gmapRoute = null;
            }
        }
    }
} 