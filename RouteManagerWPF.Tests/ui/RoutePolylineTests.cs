using Xunit;
using nl.siwoc.RouteManager.ui;
using nl.siwoc.RouteManager;

namespace nl.siwoc.RouteManager.Tests.ui
{
    public class RoutePolylineTests
    {
        [Theory]
        [InlineData("8216,8", 8216.8)]
        [InlineData("8,8", 8.8)]
        [InlineData("30", 30)]
        [InlineData("9,0", 9.0)]
        public void ParseDuration_OSM_ReturnsSeconds(string input, double expected)
        {
            // Arrange
            var polyline = new RoutePolyline(null);
            Settings.SaveRoutingProvider(Settings.OpenStreetMapProviderName);

            // Act
            var result = polyline.ParseDuration(input);

            // Assert
            Assert.Equal(expected, result, 5);
        }

        [Theory]
        [InlineData("2 hours 23 mins", 8580)] // 2*3600 + 23*60
        [InlineData("9 mins", 540)] // 9*60
        public void ParseDuration_Google_ReturnsSeconds(string input, double expected)
        {
            // Arrange
            var polyline = new RoutePolyline(null);
            Settings.SaveRoutingProvider(Settings.GoogleMapProviderName);

            // Act
            var result = polyline.ParseDuration(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ParseDuration_InvalidInput_ReturnsZero()
        {
            // Arrange
            var polyline = new RoutePolyline(null);
            Settings.SaveRoutingProvider(Settings.OpenStreetMapProviderName);

            // Act
            var result = polyline.ParseDuration("invalid");

            // Assert
            Assert.Equal(0, result);
        }
    }
} 