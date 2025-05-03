using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GMap.NET;

namespace nl.siwoc.RouteManager.fileFormats
{
    public class GpxFileParser : IFileParser
    {
        private const string GpxNamespace10 = "http://www.topografix.com/GPX/1/0";
        private const string GpxNamespace11 = "http://www.topografix.com/GPX/1/1";

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

            throw new Exception("No route or track points found in GPX file");
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
                            points.Add(point);
                        }
                    }
                    else
                    {
                        point = CreateRoutePoint(rte.lat, rte.lon, points.Count + 1, rte.name);
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

        private RoutePoint CreateRoutePoint(string lat,string lon, int index, string name)
        {
            var point = new RoutePoint(index, new PointLatLng(DegreeFromString(lat), DegreeFromString(lon)))
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
            var doc = new XDocument(
                new XElement(XNamespace.Get(ns) + "gpx",
                    new XAttribute("version", "1.1"),
                    new XAttribute("creator", "RouteManager (https://github.com/SiWoC/RouteManagerWPF)"),
                    new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                    new XAttribute(XNamespace.Xmlns + "schemaLocation", "http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd"),
                    new XElement(XNamespace.Get(ns) + "metadata",
                        new XElement(XNamespace.Get(ns) + "name", routeName ?? GetFileNameWithoutExtension(filePath))
                    ),
                    new XElement(XNamespace.Get(ns) + "rte",
                        new XElement(XNamespace.Get(ns) + "name", routeName ?? GetFileNameWithoutExtension(filePath)),
                        points.Select(p => new XElement(XNamespace.Get(ns) + "rtept",
                            new XAttribute("lat", p.Position.Lat.ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("lon", p.Position.Lng.ToString(CultureInfo.InvariantCulture)),
                            !string.IsNullOrEmpty(p.Name) ? new XElement(XNamespace.Get(ns) + "name", p.Name) : null
                        ))
                    )
                )
            );

            using (var writer = new StreamWriter(filePath, false, new System.Text.UTF8Encoding(false)))
            {
                doc.Save(writer);
            }
        }

        private double DegreeFromString(string str)
        {
            return double.Parse(str, CultureInfo.InvariantCulture);
        }
    
    }
} 