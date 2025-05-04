using System;
using System.IO;
using System.Linq;
using GMap.NET;
using nl.siwoc.RouteManager.fileFormats;
using Xunit;

namespace nl.siwoc.RouteManager.Tests.fileFormats
{
    public class VcfFileParserTests
    {
        private readonly string testDataPath = "testdata";
        private readonly VcfFileParser _parser = new VcfFileParser();

        [Fact]
        public void Read_ValidVcfFiles_ReturnsPoints()
        {
            // Arrange
            var filePath = Path.Combine(testDataPath, "destinations", "G2001 6384 Waidring, Austria Sonnwendstrasse 11.vcf");

            // Act
            var (points, routeName) = _parser.Read(filePath);

            // Assert
            Assert.Equal("G2", routeName);
            Assert.Equal(11, points.Count);
            Assert.Equal("6384 Waidring, Austria Sonnwendstrasse 11", points[0].Name);
            Assert.Equal(47.583960, points[0].Position.Lat, 6);
            Assert.Equal(12.572730, points[0].Position.Lng, 6);
            Assert.Equal("FOTOMOMENTJE", points[4].Name);
            Assert.Equal(47.122480, points[4].Position.Lat, 6);
            Assert.Equal(12.820980, points[4].Position.Lng, 6);
            Assert.True(points[10].IsFinish);
        }

        [Fact]
        public void Read_InvalidVcfFile_ThrowsException()
        {
            // Arrange
            var testOutputPath = Path.Combine(testDataPath, "test_output");
            var destinationsPath = Path.Combine(testOutputPath, "destinations");
            var filePath = Path.Combine(destinationsPath, "invalid.vcf");
            Directory.CreateDirectory(destinationsPath);
            File.WriteAllText(filePath, "BEGIN:VCARD\nEND:VCARD");

            try
            {
                // Act & Assert
                Assert.Throws<Exception>(() => _parser.Read(filePath));
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(testOutputPath))
                {
                    Directory.Delete(testOutputPath, true);
                }
            }
        }

        [Fact]
        public void Write_ValidPoints_CreatesVcfFiles()
        {
            // Arrange
            var points = new[]
            {
                new RoutePoint(1, new PointLatLng(47.583960, 12.572730)) { Name = "Test Point 1" },
                new RoutePoint(2, new PointLatLng(47.584960, 12.573730)) { Name = "Test Point 2" }
            }.ToList();
            points.Last().IsFinish = true;
            var outputPath = Path.Combine(testDataPath, "test_output");

            try
            {
                // Act
                _parser.Write(outputPath, points, "R1");

                // Assert
                var files = Directory.GetFiles(Path.Combine(outputPath, "destinations"), "R1*.vcf");
                Assert.Equal(2, files.Length);
                Assert.Contains("R1001 Test Point 1.vcf", files[0]);
                Assert.Contains("R1002 Test Point 2.vcf", files[1]);

                // Verify content of first file
                var content = File.ReadAllLines(files[0]);
                Assert.Contains("BEGIN:VCARD", content);
                Assert.Contains("VERSION:3.0", content);
                Assert.Contains("N:R1001 Test Point 1;;;;", content);
                Assert.Contains("FN:R1001 Test Point 1", content);
                Assert.Contains("GEO:47.583960;12.572730", content);
                Assert.Contains("END:VCARD", content);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(outputPath))
                {
                    Directory.Delete(outputPath, true);
                }
            }
        }

        [Fact]
        public void Write_WithLongPrefix_CreatesVcfFiles()
        {
            // Arrange
            var points = new[]
            {
                new RoutePoint(1, new PointLatLng(47.583960, 12.572730)) { Name = "Test Point 1" },
                new RoutePoint(2, new PointLatLng(47.584960, 12.573730)) { Name = "Test Point 2" }
            }.ToList();
            points.Last().IsFinish = true;
            var outputPath = Path.Combine(testDataPath, "test_output");

            try
            {
                // Act - Write
                _parser.Write(outputPath, points, "Harz1");

                // Assert - Check written files
                var files = Directory.GetFiles(Path.Combine(outputPath, "destinations"), "Harz1*.vcf");
                Assert.Equal(2, files.Length);
                Assert.Contains("Harz1001 Test Point 1.vcf", files[0]);
                Assert.Contains("Harz1002 Test Point 2.vcf", files[1]);

                // Act - Read back
                var (readPoints, routeName) = _parser.Read(files[0]);

                // Assert - Check read points
                Assert.Equal("Harz1", routeName);
                Assert.Equal(2, readPoints.Count);
                Assert.Equal("Test Point 1", readPoints[0].Name);
                Assert.Equal(47.583960, readPoints[0].Position.Lat, 6);
                Assert.Equal(12.572730, readPoints[0].Position.Lng, 6);
                Assert.Equal("Test Point 2", readPoints[1].Name);
                Assert.Equal(47.584960, readPoints[1].Position.Lat, 6);
                Assert.Equal(12.573730, readPoints[1].Position.Lng, 6);
                Assert.True(readPoints[1].IsFinish);
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(outputPath))
                {
                    Directory.Delete(outputPath, true);
                }
            }
        }
    }
} 