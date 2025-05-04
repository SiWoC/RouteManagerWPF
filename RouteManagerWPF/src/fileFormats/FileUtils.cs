using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace nl.siwoc.RouteManager.fileFormats
{
    public static class FileUtils
    {
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
        private static readonly Regex MultipleSpaces = new Regex(@"\s+", RegexOptions.Compiled);

        public static Encoding DetectEncoding(string filePath)
        {
            var buffer = new byte[4];
            using (var file = File.OpenRead(filePath))
            {
                file.Read(buffer, 0, 4);
            }

            // Check for UTF-16 LE BOM
            if (buffer[0] == 0xFF && buffer[1] == 0xFE)
                return Encoding.Unicode;

            // Default to UTF-8
            return Encoding.UTF8;
        }

        public static double ParseDouble(string value)
        {
            return double.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
        }

        public static string FormatDouble(double value, string format = "F6")
        {
            return value.ToString(format, CultureInfo.InvariantCulture);
        }

        public static bool TryParseDouble(string value, out double result)
        {
            return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
        }

        public static string EscapeVcfName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            
            // Escape special characters according to VCF 3.0 spec
            return name
                .Replace("\\", "\\\\")  // Backslash must be escaped first
                .Replace(",", "\\,")    // Comma
                .Replace(";", "\\;")    // Semicolon
                .Replace("\n", "\\n")   // Newline
                .Replace("\r", "\\r");  // Carriage return
        }

        public static string UnescapeVcfName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            
            // Unescape special characters according to VCF 3.0 spec
            return name
                .Replace("\\n", "\n")   // Newline
                .Replace("\\r", "\r")   // Carriage return
                .Replace("\\,", ",")    // Comma
                .Replace("\\;", ";")    // Semicolon
                .Replace("\\\\", "\\"); // Backslash must be unescaped last
        }

        public static string CreateWindowsSafeFileName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

            // Replace invalid characters with space
            var safeName = new string(name.Select(c => InvalidFileNameChars.Contains(c) ? ' ' : c).ToArray());
            
            // Replace multiple spaces with single space
            safeName = MultipleSpaces.Replace(safeName, " ");
            
            // Trim spaces from start and end
            safeName = safeName.Trim();
            
            // If name is empty after cleaning, use a default
            if (string.IsNullOrEmpty(safeName))
            {
                safeName = "unnamed";
            }

            return safeName;
        }
    }
}
