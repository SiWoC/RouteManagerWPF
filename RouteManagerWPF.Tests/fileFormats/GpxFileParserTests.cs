using System;
using System.IO;
using System.Linq;
using Xunit;
using nl.siwoc.RouteManager.fileFormats;

namespace nl.siwoc.RouteManager.Tests.fileFormats
{
    public class GpxFileParserTests
    {
        private readonly string testDataPath = "testdata";

        [Fact]
        public void ReadRouteFile_ShouldReadAllPoints()
        {
            // Arrange
            var parser = new GpxFileParser();
            var filePath = Path.Combine(testDataPath, "SLS 2022 voorjaarsrit 2.gpx");

            // Act
            var (points, routeName) = parser.Read(filePath);

            // Assert
            Assert.Equal(13, points.Count);
            Assert.Equal("SLS 2022 voorjaarsrit 2", routeName);
            Assert.Equal("Russ, Grand Est, Frankrijk", points[0].Name);
            Assert.Equal(48.505021, points[0].Position.Lat, 6);
            Assert.Equal(7.278500, points[0].Position.Lng, 6);
        }

        [Fact]
        public void ReadTrackFile_ShouldReadAllPoints()
        {
            // Arrange
            var parser = new GpxFileParser();
            var filePath = Path.Combine(testDataPath, "Tour vom 2020-06-12.gpx");

            // Act
            var (points, routeName) = parser.Read(filePath);

            // Assert
            Assert.True(points.Count > 100); // Track has many points
            Assert.Equal("Tour vom 2020-06-12", routeName);
            Assert.Equal(52.349891, points[0].Position.Lat, 6);
            Assert.Equal(8.230268, points[0].Position.Lng, 6);
        }

        [Fact]
        public void ReadNonExistentFile_ShouldThrowException()
        {
            // Arrange
            var parser = new GpxFileParser();
            var filePath = Path.Combine(testDataPath, "nonexistent.gpx");

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => parser.Read(filePath));
        }

        [Fact]
        public void ReadInvalidGpx_ShouldThrowException()
        {
            // Arrange
            var parser = new GpxFileParser();
            var filePath = Path.Combine(testDataPath, "invalid.gpx");
            File.WriteAllText(filePath, "<invalid>not a gpx file</invalid>");

            try
            {
                // Act & Assert
                Assert.Throws<Exception>(() => parser.Read(filePath));
            }
            finally
            {
                // Cleanup
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        [Fact]
        public void ReadWaypointsOnly_ShouldReadAllPoints()
        {
            // Arrange
            var parser = new GpxFileParser();
            var filePath = Path.Combine(testDataPath, "Test1.wptonly.gpx");

            // Act
            var (points, routeName) = parser.Read(filePath);

            // Assert
            Assert.Equal(4, points.Count);
            Assert.Equal("Test1", routeName);
            Assert.Equal("The Start", points[0].Name);
            Assert.Equal(50.451688, points[0].Position.Lat, 6);
            Assert.Equal(8.029562, points[0].Position.Lng, 6);
            Assert.Equal("Am Holzbach 3", points[2].Name);
            Assert.Equal("The Finish", points[3].Name);
            Assert.False(points[0].IsFinish);
            Assert.False(points[1].IsFinish);
            Assert.False(points[2].IsFinish);
            Assert.True(points[3].IsFinish);
        }

        [Fact]
        public void ReadRoutePointsWithStops_ShouldReadAllPointsAndStops()
        {
            // Arrange
            var parser = new GpxFileParser();
            var filePath = Path.Combine(testDataPath, "Test1.rpt.withstops.gpx");

            // Act
            var (points, routeName) = parser.Read(filePath);

            // Assert
            Assert.Equal(4, points.Count);
            Assert.Equal("Test1", routeName);
            Assert.Equal("The Start", points[0].Name);
            Assert.False(points[0].IsStop);
            Assert.False(points[0].IsFinish);
            Assert.True(points[1].IsStop);
            Assert.False(points[1].IsFinish);
            Assert.Equal("Am Holzbach 3", points[2].Name);
            Assert.False(points[2].IsStop);
            Assert.False(points[2].IsFinish);
            Assert.Equal("The Finish", points[3].Name);
            Assert.False(points[3].IsStop);
            Assert.True(points[3].IsFinish);
        }

        [Fact]
        public void ReadWaypointsAndRoutePointsWithStops_ShouldReadAllPointsAndStops()
        {
            // Arrange
            var parser = new GpxFileParser();
            var filePath = Path.Combine(testDataPath, "Test1.wpt+rpt.withstops.gpx");

            // Act
            var (points, routeName) = parser.Read(filePath);

            // Assert
            Assert.Equal(4, points.Count);
            Assert.Equal("Test1", routeName);
            Assert.Equal("The Start", points[0].Name);
            Assert.False(points[0].IsStop);
            Assert.False(points[0].IsFinish);
            Assert.True(points[1].IsStop);
            Assert.False(points[1].IsFinish);
            Assert.Equal("Am Holzbach 3", points[2].Name);
            Assert.False(points[2].IsStop);
            Assert.False(points[2].IsFinish);
            Assert.Equal("The Finish", points[3].Name);
            Assert.False(points[3].IsStop);
            Assert.True(points[3].IsFinish);
        }

        [Fact]
        public void WriteAndRead_ShouldProduceSamePoints()
        {
            // Arrange
            var parser = new GpxFileParser();
            var filePath = Path.Combine(testDataPath, "test_output.gpx");
            var originalPoints = new[]
            {
                new RoutePoint(1, new GMap.NET.PointLatLng(50.451688, 8.029562)) { Name = "The Start", IsStop = false },
                new RoutePoint(2, new GMap.NET.PointLatLng(50.447725, 8.068956)) { IsStop = true },
                new RoutePoint(3, new GMap.NET.PointLatLng(50.460410, 8.090680)) { Name = "Am Holzbach 3", IsStop = false },
                new RoutePoint(4, new GMap.NET.PointLatLng(50.462440, 8.055320)) { Name = "The Finish", IsStop = false, IsFinish = true }
            }.ToList();

            try
            {
                // Act
                parser.Write(filePath, originalPoints, "Test Route");
                var (readPoints, routeName) = parser.Read(filePath);

                // Assert
                Assert.Equal(originalPoints.Count, readPoints.Count);
                Assert.Equal("Test Route", routeName);
                for (int i = 0; i < originalPoints.Count; i++)
                {
                    Assert.Equal(originalPoints[i].Name, readPoints[i].Name);
                    Assert.Equal(originalPoints[i].Position.Lat, readPoints[i].Position.Lat, 6);
                    Assert.Equal(originalPoints[i].Position.Lng, readPoints[i].Position.Lng, 6);
                    Assert.Equal(originalPoints[i].IsStop, readPoints[i].IsStop);
                }
                Assert.False(readPoints[0].IsFinish);
                Assert.False(readPoints[1].IsFinish);
                Assert.False(readPoints[2].IsFinish);
                Assert.True(readPoints[3].IsFinish);
            }
            finally
            {
                // Cleanup
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }
    }
} 