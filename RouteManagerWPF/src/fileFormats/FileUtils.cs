using System.IO;
using System.Text;

namespace nl.siwoc.RouteManager.fileFormats
{
    public static class FileUtils
    {
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
    }
}
