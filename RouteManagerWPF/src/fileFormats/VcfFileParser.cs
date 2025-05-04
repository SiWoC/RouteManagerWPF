using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GMap.NET;

namespace nl.siwoc.RouteManager.fileFormats
{
    public class VcfFileParser : IFileParser
    {
        private static readonly Encoding VcfEncoding = new UTF8Encoding(false); // No BOM
        private static readonly Regex PrefixRegex = new Regex(@"^([A-Za-z]+)(\d)(\d+)(?=\s)", RegexOptions.Compiled);

        public string[] SupportedFileTypes => new[] { ".vcf" };

        public (List<RoutePoint> Points, string RouteName) Read(string filePath)
        {
            var points = new List<RoutePoint>();
            var directory = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var prefixMatch = PrefixRegex.Match(fileName);
            var prefix = prefixMatch.Success ? prefixMatch.Groups[1].Value + prefixMatch.Groups[2].Value : fileName.Substring(0, 2);

            // Find all VCF files with the same prefix in the directory
            var vcfFiles = Directory.GetFiles(directory, $"{prefix}*.vcf")
                                  .OrderBy(f => f)
                                  .ToList();

            foreach (var vcfFile in vcfFiles)
            {
                var lines = File.ReadAllLines(vcfFile, VcfEncoding);
                var geoLine = lines.FirstOrDefault(l => l.StartsWith("GEO:"));
                var nameLine = lines.FirstOrDefault(l => l.StartsWith("FN:"));

                if (geoLine != null)
                {
                    var geoParts = geoLine.Substring(4).Split(';');
                    if (geoParts.Length == 2 && 
                        FileUtils.TryParseDouble(geoParts[0], out double lat) && 
                        FileUtils.TryParseDouble(geoParts[1], out double lng))
                    {
                        var position = new PointLatLng(lat, lng);
                        var point = new RoutePoint(points.Count + 1, position);
                        
                        if (nameLine != null)
                        {
                            var name = FileUtils.UnescapeVcfName(nameLine.Substring(3));
                            // Remove prefix and index from name
                            var nameMatch = PrefixRegex.Match(name);
                            point.Name = nameMatch.Success ? name.Substring(nameMatch.Length + 1) : name;
                        }
                        else
                        {
                            var name = Path.GetFileNameWithoutExtension(vcfFile);
                            var nameMatch = PrefixRegex.Match(name);
                            point.Name = nameMatch.Success ? name.Substring(nameMatch.Length + 1) : name;
                        }

                        points.Add(point);
                    }
                }
            }

            if (points.Count == 0)
            {
                throw new Exception("No valid points found in VCF files");
            }

            points.Last().IsFinish = true;
            return (points, prefix);
        }

        public void Write(string filePath, List<RoutePoint> points, string routeName = null)
        {
            if (points == null || points.Count == 0)
            {
                throw new ArgumentException("Cannot write empty points list", nameof(points));
            }

            if (filePath.EndsWith("vcf", StringComparison.OrdinalIgnoreCase))
            {
                filePath = Path.GetDirectoryName(filePath);
            }
            if (!filePath.EndsWith("destinations", StringComparison.OrdinalIgnoreCase))
            {
                filePath = Path.Combine(filePath, "destinations");
            }
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            var prefix = routeName ?? "R1"; // Default prefix if none provided
            var index = 1;

            foreach (var point in points)
            {
                var safeFileName = FileUtils.CreateWindowsSafeFileName(point.Name);
                var vcfPath = Path.Combine(filePath, $"{prefix}{index:D3} {safeFileName}.vcf");
                
                // Add prefix and index to the name in VCF
                var fullName = $"{prefix}{index:D3} {point.Name}";
                var escapedName = FileUtils.EscapeVcfName(fullName);
                
                var vcfContent = new List<string>
                {
                    "BEGIN:VCARD",
                    "VERSION:3.0",
                    $"N:{escapedName};;;;",
                    $"FN:{escapedName}",
                    $"GEO:{FileUtils.FormatDouble(point.Position.Lat)};{FileUtils.FormatDouble(point.Position.Lng)}",
                    "END:VCARD"
                };

                File.WriteAllLines(vcfPath, vcfContent, VcfEncoding);
                index++;
            }
        }
    }
} 