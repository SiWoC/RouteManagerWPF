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
        public void WriteAndRead_ShouldProduceSamePoints()
        {
            // Arrange
            var parser = new GpxFileParser();
            var filePath = Path.Combine(testDataPath, "test_output.gpx");
            var originalPoints = new[]
            {
                new RoutePoint(1, new GMap.NET.PointLatLng(52.0, 4.0)) { Name = "Point 1" },
                new RoutePoint(2, new GMap.NET.PointLatLng(52.1, 4.1)) { Name = "Point 2" }
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
                }
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