using System.IO;
using Xunit;
using nl.siwoc.RouteManager.fileFormats;

namespace nl.siwoc.RouteManager.Tests.fileFormats
{
    public class CoPilotTrpFileParserTests
    {
        private readonly string testDataPath = "testdata";

        [Fact]
        public void ReadCoPilot9Trp_ShouldReadAllPoints()
        {
            // Arrange
            var parser = new CoPilotTrpFileParser();
            var filePath = Path.Combine(testDataPath, "Cols Vogezen.trp");

            // Act
            var (points, routeName) = parser.Read(filePath);

            // Assert
            Assert.Equal(13, points.Count);
            Assert.Equal("Cols Vogezen", routeName);
            Assert.Equal("D1420", points[0].Name);
            Assert.Equal(48.50517, points[0].Position.Lat, 6);
            Assert.Equal(7.27898, points[0].Position.Lng, 6);
            Assert.Equal("Russ", points[0].City);
            Assert.Equal("France", points[0].Country);
            Assert.Equal("67130", points[0].Zip);
        }

        [Fact]
        public void ReadCoPilot9Trp_StopOptions()
        {
            // Arrange
            var parser = new CoPilotTrpFileParser();
            var filePath = Path.Combine(testDataPath, "SLS.Harz.2025.dag.1.trp");

            // Act
            var (points, routeName) = parser.Read(filePath);

            // Assert
            Assert.Equal(11, points.Count);
            Assert.Equal("SLS Harz 2025 dag 1", routeName);
            Assert.Equal("Wellerberg 2", points[1].Name);
            Assert.Equal(51.473040, points[1].Position.Lat, 6);
            Assert.Equal(9.109070, points[1].Position.Lng, 6);
            Assert.Equal("Germany-34414 Warburg", points[1].City);
        }

        [Fact]
        public void ReadCoPilot6_7Trp_ShouldReadAllPoints()
        {
            // Arrange
            var parser = new CoPilotTrpFileParser();
            var filePath = Path.Combine(testDataPath, "Test copilot 6-7.trp");

            // Act
            var (points, routeName) = parser.Read(filePath);

            // Assert
            Assert.Equal(4, points.Count);
            Assert.Equal("Test copilot 6", routeName);
            Assert.Equal("GPS-plaatsbepaling", points[0].City);
            Assert.Equal(52.145097, points[0].Position.Lat, 6);
            Assert.Equal(4.668902, points[0].Position.Lng, 6);
        }

        [Fact]
        public void ReadCoPilot8Trp_ShouldReadAllPoints()
        {
            // Arrange
            var parser = new CoPilotTrpFileParser();
            var filePath = Path.Combine(testDataPath, "Test copilot 8-9.trp");

            // Act
            var (points, routeName) = parser.Read(filePath);

            // Assert
            Assert.Equal(4, points.Count);
            Assert.Equal("Test copilot 8", routeName);
            Assert.Equal("GPS-plaatsbepaling", points[0].City);
            Assert.Equal(52.145097, points[0].Position.Lat, 6);
            Assert.Equal(4.668902, points[0].Position.Lng, 6);
        }

        [Fact]
        public void ReadNonExistentFile_ShouldThrowException()
        {
            // Arrange
            var parser = new CoPilotTrpFileParser();
            var filePath = Path.Combine(testDataPath, "nonexistent.trp");

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => parser.Read(filePath));
        }

        [Fact]
        public void ReadInvalidTrp_ShouldThrowException()
        {
            // Arrange
            var parser = new CoPilotTrpFileParser();
            var filePath = Path.Combine(testDataPath, "invalid.trp");
            File.WriteAllText(filePath, "invalid trp file");

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
        public void ReadJsonFormatTrp_ShouldThrowException()
        {
            // Arrange
            var parser = new CoPilotTrpFileParser();
            var filePath = Path.Combine(testDataPath, "Test copilot 10 RC.pretty.trp");

            // Act & Assert
            Assert.Throws<Exception>(() => parser.Read(filePath));
        }

        [Fact]
        public void WriteAndRead_ShouldProduceSamePoints()
        {
            // Arrange
            var parser = new CoPilotTrpFileParser();
            var filePath = Path.Combine(testDataPath, "test_output.trp");
            var originalPoints = new[]
            {
                new RoutePoint(1, new GMap.NET.PointLatLng(52.0, 4.0)) 
                { 
                    Name = "Point 1",
                    City = "Amsterdam",
                    Country = "NL",
                    Zip = "1234 AB"
                },
                new RoutePoint(2, new GMap.NET.PointLatLng(52.1, 4.1)) 
                { 
                    Name = "Point 2",
                    City = "Rotterdam",
                    Country = "NL",
                    Zip = "5678 CD"
                }
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
                    Assert.Equal(originalPoints[i].Position.Lat, readPoints[i].Position.Lat, 5);
                    Assert.Equal(originalPoints[i].Position.Lng, readPoints[i].Position.Lng, 5);
                    Assert.Equal(originalPoints[i].City, readPoints[i].City);
                    Assert.Equal(originalPoints[i].Country, readPoints[i].Country);
                    Assert.Equal(originalPoints[i].Zip, readPoints[i].Zip);
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