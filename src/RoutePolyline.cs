using GMap.NET;
using GMap.NET.WindowsPresentation;
using System.Windows.Media;
using System.Windows.Shapes;
using GMap.NET.MapProviders;

namespace nl.siwoc.RouteManager
{
    public class RoutePolyline
    {
        private readonly MapControlWrapper mapControl;
        private GMapRoute gmapRoute;
        private RoutingProvider rp;

        public RoutePolyline(MapControlWrapper mapControl)
        {
            this.mapControl = mapControl;
            rp = mapControl.MapProvider as RoutingProvider;
            if (rp == null)
            {
                rp = GMapProviders.OpenStreetMap; // use OpenStreetMap if provider does not implement routing
            }
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
                
                MapRoute mapRoute = rp.GetRoute(start, end, false, false, (int)mapControl.Zoom);
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