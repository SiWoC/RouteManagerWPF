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
        private List<GMapRoute> routeSegments = new List<GMapRoute>();
        private List<GMapRoute> tempSegments = new List<GMapRoute>();

        public enum RouteStyle
        {
            Normal,
            Temporary
        }

        private static Path GetRoutePath(RouteStyle style) => style switch
        {
            RouteStyle.Normal => new Path
            {
                Stroke = Brushes.Blue,
                StrokeThickness = 4,
                Opacity = 0.7
            },
            RouteStyle.Temporary => new Path
            {
                Stroke = Brushes.Red,
                StrokeThickness = 4,
                Opacity = 0.5
            },
            _ => throw new ArgumentException($"Unknown route style: {style}")
        };

        public RoutePolyline(MapControlWrapper mapControl)
        {
            this.mapControl = mapControl;
        }

        public async Task<(double distance, double duration)> UpdateRoute(IEnumerable<RoutePoint> points)
        {
            var pointList = points.ToList();
            if (pointList.Count < 2)
            {
                await Application.Current.Dispatcher.InvokeAsync(() => {
                    foreach (var segment in routeSegments)
                    {
                        mapControl.Markers.Remove(segment);
                    }
                    routeSegments.Clear();
                });
                return (0, 0);
            }

            return await UpdateSegments(points, 0, pointList.Count - 1);
        }

        private async Task<(double distance, double duration)> UpdateSegments(IEnumerable<RoutePoint> points, int startIndex, int endIndex, bool isTemporary = false)
        {
            double totalDistance = 0;
            double totalDuration = 0;
            var pointList = points.ToList();
            if (pointList.Count < 2) return (0, 0);

            long startTime = DateTime.UtcNow.Ticks;

            // Calculate segments between startIndex and endIndex
            for (int i = startIndex; i < endIndex; i++)
            {
                var start = pointList[i].Position;
                var end = pointList[i + 1].Position;
                
                var zoom = await Application.Current.Dispatcher.InvokeAsync(() => (int)mapControl.Zoom);
                MapRoute mapRoute = await Task.Run(() => Settings.LoadRoutingProvider().GetRoute(start, end, false, false, zoom));
                if (mapRoute != null)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() => {
                        var segment = new GMapRoute(new List<PointLatLng>(mapRoute.Points));
                        segment.Shape = GetRoutePath(isTemporary ? RouteStyle.Temporary : RouteStyle.Normal);
                        segment.Tag = startTime;

                        if (isTemporary)
                        {
                            tempSegments.Add(segment);
                        }
                        else
                        {
                            if (i < routeSegments.Count)
                            {
                                mapControl.Markers.Remove(routeSegments[i]);
                                routeSegments[i] = segment;
                            }
                            else
                            {
                                routeSegments.Add(segment);
                            }
                        }
                        mapControl.Markers.Add(segment);
                    });

                    totalDistance += mapRoute.Distance;
                    totalDuration += ParseDuration(mapRoute.Duration);
                    pointList[i + 1].CumulativeDistance = totalDistance;
                    pointList[i + 1].CumulativeDuration = Utils.ConvertRouteDuration(totalDuration);
                }
            }

            if (isTemporary)
            {
                await Application.Current.Dispatcher.InvokeAsync(() => {
                    // Remove any temporary segments that are older than our current batch
                    var segmentsToRemove = tempSegments.Where(s => (long)s.Tag < startTime).ToList();
                    foreach (var segment in segmentsToRemove)
                    {
                        mapControl.Markers.Remove(segment);
                        tempSegments.Remove(segment);
                    }
                });
            }

            return (totalDistance, totalDuration);
        }

        public async Task<(double distance, double duration)> UpdateRouteForDraggedPoint(IEnumerable<RoutePoint> points, int draggedIndex)
        {
            var pointList = points.ToList();
            if (pointList.Count < 2) return (0, 0);

            // Clear previous temporary segments
            await Application.Current.Dispatcher.InvokeAsync(() => {
                foreach (var segment in tempSegments)
                {
                    mapControl.Markers.Remove(segment);
                }
                tempSegments.Clear();
            });

            // Update segments before and after the dragged point
            int startIndex = Math.Max(0, draggedIndex - 1);
            int endIndex = Math.Min(pointList.Count - 1, draggedIndex + 1);

            return await UpdateSegments(points, startIndex, endIndex, true);
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
            foreach (var segment in routeSegments)
            {
                mapControl.Markers.Remove(segment);
            }
            routeSegments.Clear();
            foreach (var segment in tempSegments)
            {
                mapControl.Markers.Remove(segment);
            }
            tempSegments.Clear();
        }

        public void ClearTemporarySegments()
        {
            Application.Current.Dispatcher.Invoke(() => {
                foreach (var segment in tempSegments)
                {
                    mapControl.Markers.Remove(segment);
                }
                tempSegments.Clear();
            });
        }
    }
} 