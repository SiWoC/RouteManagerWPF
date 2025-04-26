using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.Windows.Media;
using System.Windows.Shapes;
using GMap.NET.MapProviders;
using nl.siwoc.RouteManager;

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

        public void UpdateRoute(IEnumerable<RoutePoint> points)
        {
            var pointList = points.ToList();
            if (pointList.Count < 2)
            {
                if (gmapRoute != null)
                {
                    mapControl.Markers.Remove(gmapRoute);
                    gmapRoute = null;
                }
                return;
            }

            var allPoints = new List<PointLatLng>();
            for (int i = 0; i < pointList.Count - 1; i++)
            {
                var start = pointList[i].Position;
                var end = pointList[i + 1].Position;
                
                MapRoute mapRoute = Settings.LoadRoutingProvider().GetRoute(start, end, false, false, (int)mapControl.Zoom);
                if (mapRoute != null)
                {
                    allPoints.AddRange(mapRoute.Points);
                }
            }

            if (allPoints.Count > 0)
            {
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
            }
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