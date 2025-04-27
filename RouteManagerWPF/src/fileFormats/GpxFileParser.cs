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

            // First try to read route points
            var (routePoints, routeNameFromRoute) = ReadRoutePoints(gpx, ns);
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

        private (List<RoutePoint> Points, string RouteName) ReadRoutePoints(XElement gpx, XNamespace ns)
        {
            var points = new List<RoutePoint>();
            var routes = gpx.Elements(ns + "rte");
            string routeName = null;

            foreach (var route in routes)
            {
                routeName = route.Element(ns + "name")?.Value;
                var routePoints = route.Elements(ns + "rtept");

                foreach (var rtept in routePoints)
                {
                    var lat = double.Parse(rtept.Attribute("lat").Value, CultureInfo.InvariantCulture);
                    var lon = double.Parse(rtept.Attribute("lon").Value, CultureInfo.InvariantCulture);
                    var name = rtept.Element(ns + "name")?.Value;

                    var point = new RoutePoint(points.Count + 1, new PointLatLng(lat, lon))
                    {
                        Name = name
                    };

                    points.Add(point);
                }
            }

            return (points, routeName);
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
                    
                    // If we have more than 100 points, sample evenly
                    if (trackPoints.Count > 100)
                    {
                        var step = (double)(trackPoints.Count - 1) / 100;
                        for (double i = 0; i < trackPoints.Count - 1; i += step)
                        {
                            var trkpt = trackPoints[(int)i];
                            var lat = double.Parse(trkpt.Attribute("lat").Value, CultureInfo.InvariantCulture);
                            var lon = double.Parse(trkpt.Attribute("lon").Value, CultureInfo.InvariantCulture);
                            var name = trkpt.Element(ns + "name")?.Value;
                            points.Add(new RoutePoint(points.Count + 1, new PointLatLng(lat, lon)) { Name = name });
                        }

                        // Always include last point
                        var lastPoint = trackPoints[trackPoints.Count - 1];
                        var lastLat = double.Parse(lastPoint.Attribute("lat").Value, CultureInfo.InvariantCulture);
                        var lastLon = double.Parse(lastPoint.Attribute("lon").Value, CultureInfo.InvariantCulture);
                        var lastName = lastPoint.Element(ns + "name")?.Value;
                        points.Add(new RoutePoint(points.Count + 1, new PointLatLng(lastLat, lastLon)) { Name = lastName });
                    }
                    else
                    {
                        foreach (var trkpt in trackPoints)
                        {
                            var lat = double.Parse(trkpt.Attribute("lat").Value, CultureInfo.InvariantCulture);
                            var lon = double.Parse(trkpt.Attribute("lon").Value, CultureInfo.InvariantCulture);
                            var name = trkpt.Element(ns + "name")?.Value;

                            var point = new RoutePoint(points.Count + 1, new PointLatLng(lat, lon))
                            {
                                Name = name
                            };

                            points.Add(point);
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
    }
} 