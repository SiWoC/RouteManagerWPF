using System;
using System.Text;
using nl.siwoc.RouteManager.fileFormats;
using Xunit;

namespace nl.siwoc.RouteManager.Tests.fileFormats
{
    public class FileUtilsTests
    {
        [Fact]
        public void ConvertToAscii_WithGermanChars_ConvertsCorrectly()
        {
            // Arrange
            var input = "Straße äöü ÄÖÜ éèêë ÉÈÊË àâ ÀÂ îï ÎÏ ô Ô ùû ÙÛ ç Ç ñ Ñ å Å æ Æ ø Ø œ Œ";
            var expected = "Strasse aou AOU eeee EEEE aa AA ii II o O uu UU c C n N a A ae AE oe OE oe OE";

            // Act
            var result = FileUtils.ConvertToAscii(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConvertToAscii_WithLatinizedChars_ConvertsCorrectly()
        {
            // Arrange
            var input = "å Å æ Æ ø Ø œ Œ";
            var expected = "a A ae AE oe OE oe OE";

            // Act
            var result = FileUtils.ConvertToAscii(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConvertToAscii_WithNull_ReturnsNull()
        {
            // Act
            var result = FileUtils.ConvertToAscii(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void ConvertToAscii_WithEmptyString_ReturnsEmptyString()
        {
            // Act
            var result = FileUtils.ConvertToAscii("");

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void ConvertToAscii_WithAsciiOnly_ReturnsSameString()
        {
            // Arrange
            var input = "Hello World 123";

            // Act
            var result = FileUtils.ConvertToAscii(input);

            // Assert
            Assert.Equal(input, result);
        }
    }
} 