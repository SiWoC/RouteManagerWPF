using System.Collections.Generic;

namespace nl.siwoc.RouteManager.fileFormats
{
    public interface IFileParser
    {
        (List<RoutePoint> Points, string RouteName) Read(string filePath);
        void Write(string filePath, List<RoutePoint> points, string routeName = null);
    }
} 