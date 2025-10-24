using GMap.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nl.siwoc.RouteManager
{
    public class RouteSegmentsFactory
    {
        private static Dictionary<string, Dictionary<SegmentKey, MapRoute>> routeProviderTable = new Dictionary<string, Dictionary<SegmentKey, MapRoute>>();

        public static event EventHandler RoutingApiCallMade;

        public static MapRoute GetRouteSegment(PointLatLng start, PointLatLng end, int zoom)
        {
            string routingProviderName = Settings.GetRoutingProviderName();
            
            if (!routeProviderTable.TryGetValue(routingProviderName, out Dictionary<SegmentKey, MapRoute> cachedSegments))
            {
                cachedSegments = new Dictionary<SegmentKey, MapRoute>();
                routeProviderTable[routingProviderName] = cachedSegments;
            }
            
            var segmentKey = new SegmentKey(start, end, zoom);
            
            if (!cachedSegments.TryGetValue(segmentKey, out MapRoute mapRoute))
            {
                mapRoute = Settings.GetRoutingProvider().GetRoute(start, end, false, false, zoom);
                cachedSegments[segmentKey] = mapRoute;
                RoutingApiCallMade?.Invoke(null, EventArgs.Empty);
            }
            
            return mapRoute;
        }

        private class SegmentKey : IEquatable<SegmentKey>
        {
            public PointLatLng Start { get; }
            public PointLatLng End { get; }
            public int Zoom { get; }

            public SegmentKey(PointLatLng start, PointLatLng end, int zoom)
            {
                Start = start;
                End = end;
                Zoom = zoom;
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as SegmentKey);
            }

            public bool Equals(SegmentKey other)
            {
                if (other == null) return false;
                return Start.Equals(other.Start) && End.Equals(other.End) && Zoom == other.Zoom;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Start, End, Zoom);
            }

            public static bool operator ==(SegmentKey left, SegmentKey right)
            {
                return EqualityComparer<SegmentKey>.Default.Equals(left, right);
            }

            public static bool operator !=(SegmentKey left, SegmentKey right)
            {
                return !(left == right);
            }
        }
    }
}
