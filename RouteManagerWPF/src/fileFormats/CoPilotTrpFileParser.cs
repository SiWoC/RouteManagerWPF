using System.IO;
using GMap.NET;

namespace nl.siwoc.RouteManager.fileFormats
{
    public class CoPilotTrpFileParser : IFileParser
    {
        public string[] SupportedFileTypes => new[] { ".trp" };

        public (List<RoutePoint> Points, string RouteName) Read(string filePath)
        {
            var points = new List<RoutePoint>();
            var encoding = FileUtils.DetectEncoding(filePath);
            var lines = File.ReadAllLines(filePath, encoding);
            var currentStop = new Dictionary<string, string>();
            string tripName = null;
            RoutePoint point = null;

            foreach (var line in lines)
            {
                if (line.StartsWith("Start Trip="))
                {
                    tripName = line.Substring("Start Trip=".Length);
                }
                else if (line.StartsWith("Start Stop="))
                {
                    currentStop.Clear();
                }
                else if (line.Equals("End Stop"))
                {
                    if (currentStop.TryGetValue("Longitude", out var lonStr) && 
                        currentStop.TryGetValue("Latitude", out var latStr) &&
                        int.TryParse(lonStr, out var lon) && 
                        int.TryParse(latStr, out var lat))
                    {
                        var position = new PointLatLng(lat / 1000000.0, lon / 1000000.0);
                        point = new RoutePoint(points.Count + 1, position);

                        if (currentStop.TryGetValue("Name", out var name))
                            point.Name = name;
                        if (currentStop.TryGetValue("Address", out var address))
                            point.Address = address;
                        if (currentStop.TryGetValue("City", out var city))
                            point.City = city;
                        if (currentStop.TryGetValue("State", out var state))
                            point.Country = state;
                        if (currentStop.TryGetValue("Zip", out var zip))
                            point.Zip = zip;
                        if (currentStop.TryGetValue("Show", out var show))
                            point.IsStop = (show == "1");

                        points.Add(point);
                        currentStop.Clear();
                    }
                }
                else if (line.Contains("="))
                {
                    var parts = line.Split('=', 2);
                    if (parts.Length == 2)
                        currentStop[parts[0]] = parts[1];
                }
            }

            if (points.Count == 0)
            {
                throw new Exception("No valid points found in TRP file");
            }
            point.IsFinish = true;

            return (points, tripName);
        }

        public void Write(string filePath, List<RoutePoint> points, string routeName = null)
        {
            if (points == null || points.Count == 0)
            {
                throw new ArgumentException("Cannot write empty points list", nameof(points));
            }

            var lines = new List<string>
            {
                "Data Version:3.4.1.2",
                $"Start Trip={routeName ?? Path.GetFileNameWithoutExtension(filePath)}",
                "End Trip",
                ""
            };

            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];
                lines.Add($"Start Stop=Stop {i}");
                lines.Add($"Longitude={(int)(point.Position.Lng * 1000000)}");
                lines.Add($"Latitude={(int)(point.Position.Lat * 1000000)}");
                if (!string.IsNullOrEmpty(point.Country))
                    lines.Add($"State={point.Country}");
                if (!string.IsNullOrEmpty(point.Zip))
                    lines.Add($"Zip={point.Zip}");
                if (!string.IsNullOrEmpty(point.City))
                    lines.Add($"City={point.City}");
                if (!string.IsNullOrEmpty(point.Name))
                    lines.Add($"Name={point.Name}");
                if (!string.IsNullOrEmpty(point.Address))
                    lines.Add($"Address={point.Address}");
                lines.Add($"Show={(point.IsStop ? "1" : "0")}");
                lines.Add("End Stop");
                lines.Add("");
            }

            File.WriteAllLines(filePath, lines, System.Text.Encoding.Unicode);
        }
    }
} 