using System.IO;
using System.Xml.Linq;
using GMap.NET;

namespace nl.siwoc.RouteManager.fileFormats
{
    public class GpxFileParser : IFileParser
    {
        public string[] SupportedFileTypes => new[] { ".gpx" };

        private const string GpxNamespace11 = "http://www.topografix.com/GPX/1/1";
        private const string RouteManagerNamespace = "https://github.com/SiWoC/RouteManagerWPF/gpx/1.0";

        private class WptType
        {
            public string lat;
            public string lon;
            public string name;

            public WptType(string lat, string lon, string name)
            {
                this.lat = lat;
                this.lon = lon;
                this.name = name;
            }

            public bool Equals(WptType other)
            {
                return lat.Equals(other.lat) && lon.Equals(other.lon);
            }
        }

        public (List<RoutePoint> Points, string RouteName) Read(string filePath)
        {
            var doc = XDocument.Load(filePath);
            var gpx = doc.Root;
            var ns = gpx.GetDefaultNamespace();

            // Get route name from metadata if available (GPX 1.1)
            string routeName = null;
            if (ns == GpxNamespace11)
            {
                var metadata = gpx.Element(ns + "metadata");
                routeName = metadata?.Element(ns + "name")?.Value;
            }

            // First try to read waypoints
            var wayPoints = ReadWayPoints(gpx, ns);

            // First try to read route points
            var (routePoints, routeNameFromRoute) = ReadRoutePoints(gpx, ns, wayPoints);
            if (routePoints.Any())
            {
                return (routePoints, routeName ?? routeNameFromRoute ?? GetFileNameWithoutExtension(filePath));
            }

            // If no route points, try to read track points
            var (trackPoints, routeNameFromTrack) = ReadTrackPoints(gpx, ns);
            if (trackPoints.Any())
            {
                return (trackPoints, routeName ?? routeNameFromTrack ?? GetFileNameWithoutExtension(filePath));
            }

            // If no route or track points, but we have waypoints, return those
            if (wayPoints.Any())
            {
                var points = wayPoints.Select((wpt, index) => 
                    new RoutePoint(index + 1, new PointLatLng(FileUtils.ParseDouble(wpt.lat), FileUtils.ParseDouble(wpt.lon)))
                    {
                        Name = wpt.name
                    }).ToList();
                if (points.Any())
                {
                    points.Last().IsFinish = true;
                }
                return (points, routeName ?? GetFileNameWithoutExtension(filePath));
            }

            throw new Exception("No route, track, or waypoints found in GPX file");
        }

        private List<WptType> ReadWayPoints(XElement gpx, XNamespace ns)
        {
            var points = new List<WptType>();

            var wpts = gpx.Elements(ns + "wpt");

            foreach (var wpt in wpts)
            {
                points.Add(new WptType(wpt.Attribute("lat").Value, wpt.Attribute("lon").Value, wpt.Element(ns + "name")?.Value));
            }

            return points;
        }

        private (List<RoutePoint> Points, string RouteName) ReadRoutePoints(XElement gpx, XNamespace ns, List<WptType> wayPoints)
        {
            var points = new List<RoutePoint>();
            var routes = gpx.Elements(ns + "rte");
            string routeName = null;

            foreach (var route in routes)
            {
                routeName = route.Element(ns + "name")?.Value;
                var routePoints = route.Elements(ns + "rtept");
                RoutePoint point = null;

                foreach (var rtept in routePoints)
                {
                    var rte = new WptType(rtept.Attribute("lat").Value, rtept.Attribute("lon").Value, rtept.Element(ns + "name")?.Value);
                    // when waypoints defined, only add waypoints in order of this route
                    if (wayPoints.Count > 0)
                    {
                        var wpt = wayPoints.Find(r => r.Equals(rte));
                        if (wpt != null)
                        {
                            point = CreateRoutePoint(wpt.lat, wpt.lon, points.Count + 1, wpt.name);
                            ReadIsStop(rtept, point);
                            points.Add(point);
                        }
                    }
                    else
                    {
                        point = CreateRoutePoint(rte.lat, rte.lon, points.Count + 1, rte.name);
                        ReadIsStop(rtept, point);
                        points.Add(point);
                    }
                }
                if (point != null)
                {
                    point.IsFinish = true;
                }
            }

            return (points, routeName);
        }

        private void ReadIsStop(XElement point, RoutePoint routePoint)
        {
            var extensions = point.Element(XNamespace.Get(GpxNamespace11) + "extensions");
            if (extensions != null)
            {
                var rmNs = XNamespace.Get(RouteManagerNamespace);
                var isStop = extensions.Element(rmNs + "isstop");
                if (isStop != null && bool.TryParse(isStop.Value, out bool value))
                {
                    routePoint.IsStop = value;
                }
            }
        }

        private RoutePoint CreateRoutePoint(string lat,string lon, int index, string name)
        {
            var point = new RoutePoint(index, new PointLatLng(FileUtils.ParseDouble(lat), FileUtils.ParseDouble(lon)))
            {
                Name = name
            };
            return point;
        }

        private (List<RoutePoint> Points, string RouteName) ReadTrackPoints(XElement gpx, XNamespace ns)
        {
            var points = new List<RoutePoint>();
            var tracks = gpx.Elements(ns + "trk");
            string routeName = null;

            foreach (var track in tracks)
            {
                routeName = track.Element(ns + "name")?.Value;
                var segments = track.Elements(ns + "trkseg");

                foreach (var segment in segments)
                {
                    var trackPoints = segment.Elements(ns + "trkpt").ToList();
                    XElement trkpt;
                    RoutePoint point = null;

                    // If we have more than 100 points, sample evenly
                    if (trackPoints.Count > 100)
                    {
                        var step = (double)(trackPoints.Count - 1) / 100;
                        for (double i = 0; i < trackPoints.Count - 1; i += step)
                        {
                            trkpt = trackPoints[(int)i];
                            point = CreateRoutePoint(trkpt.Attribute("lat").Value, trkpt.Attribute("lon").Value, points.Count + 1, trkpt.Element(ns + "name")?.Value);
                            points.Add(point);
                        }

                        // Always include last point
                        trkpt = trackPoints[trackPoints.Count - 1];
                        point = CreateRoutePoint(trkpt.Attribute("lat").Value, trkpt.Attribute("lon").Value, points.Count + 1, trkpt.Element(ns + "name")?.Value);
                        point.IsFinish = true;
                        points.Add(point);
                    }
                    else
                    {
                        foreach (var trkpt1 in trackPoints)
                        {
                            point = CreateRoutePoint(trkpt1.Attribute("lat").Value, trkpt1.Attribute("lon").Value, points.Count + 1, trkpt1.Element(ns + "name")?.Value);
                            points.Add(point);
                        }
                        if (point != null)
                        {
                            point.IsFinish = true;
                        }
                    }
                }
            }

            return (points, routeName);
        }

        private string GetFileNameWithoutExtension(string filePath)
        {
            return Path.GetFileNameWithoutExtension(filePath);
        }

        public void Write(string filePath, List<RoutePoint> points, string routeName = null)
        {
            var ns = GpxNamespace11;
            var rmNs = RouteManagerNamespace;
            var doc = new XDocument(
                new XElement(XNamespace.Get(ns) + "gpx",
                    new XAttribute("version", "1.1"),
                    new XAttribute("creator", "RouteManager (https://github.com/SiWoC/RouteManagerWPF)"),
                    new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                    new XAttribute(XNamespace.Xmlns + "srm", rmNs),
                    new XAttribute(XNamespace.Xmlns + "schemaLocation", "http://www.topografix.com/GPX/1/1/gpx.xsd"),
                    new XElement(XNamespace.Get(ns) + "metadata",
                        new XElement(XNamespace.Get(ns) + "name", routeName ?? GetFileNameWithoutExtension(filePath))
                    ),
                    // Write waypoints
                    points.Select(p => new XElement(XNamespace.Get(ns) + "wpt",
                        new XAttribute("lat", FileUtils.FormatDouble(p.Position.Lat)),
                        new XAttribute("lon", FileUtils.FormatDouble(p.Position.Lng)),
                        !string.IsNullOrEmpty(p.Name) ? new XElement(XNamespace.Get(ns) + "name", p.Name) : null,
                        new XElement(XNamespace.Get(ns) + "extensions",
                            new XElement(XNamespace.Get(rmNs) + "isstop", p.IsStop.ToString().ToLower())
                        )
                    )),
                    // Write route
                    new XElement(XNamespace.Get(ns) + "rte",
                        new XElement(XNamespace.Get(ns) + "name", routeName ?? GetFileNameWithoutExtension(filePath)),
                        points.Select(p => new XElement(XNamespace.Get(ns) + "rtept",
                            new XAttribute("lat", FileUtils.FormatDouble(p.Position.Lat)),
                            new XAttribute("lon", FileUtils.FormatDouble(p.Position.Lng)),
                            !string.IsNullOrEmpty(p.Name) ? new XElement(XNamespace.Get(ns) + "name", p.Name) : null,
                            new XElement(XNamespace.Get(ns) + "extensions",
                                new XElement(XNamespace.Get(rmNs) + "isstop", p.IsStop.ToString().ToLower())
                            )
                        ))
                    )
                )
            );

            using (var writer = new StreamWriter(filePath, false, new System.Text.UTF8Encoding(false)))
            {
                doc.Save(writer);
            }
        }

    }
} 