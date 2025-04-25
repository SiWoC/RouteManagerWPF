using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Reflection;
using System.Collections.Generic;

namespace nl.siwoc.RouteManager
{
    public partial class IconConverter : Window
    {
        public IconConverter()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Load XAML from resource
                var resourceUri = new Uri("pack://application:,,,/src/ui/AppIcon.xaml");
                using (var stream = Application.GetResourceStream(resourceUri).Stream)
                {
                    var viewbox = XamlReader.Load(stream) as Viewbox;
                    if (viewbox == null)
                    {
                        MessageBox.Show("Failed to load icon content");
                        return;
                    }

                    // Create all size variants
                    var sizes = new[] { 16, 32, 48, 64, 128, 256 };
                    var imageData = new List<byte[]>();
                    
                    foreach (var size in sizes)
                    {
                        viewbox.Width = size;
                        viewbox.Height = size;
                        viewbox.Measure(new Size(size, size));
                        viewbox.Arrange(new Rect(0, 0, size, size));
                        viewbox.UpdateLayout();

                        var rtb = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
                        rtb.Render(viewbox);

                        using (var ms = new MemoryStream())
                        {
                            var encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(rtb));
                            encoder.Save(ms);
                            imageData.Add(ms.ToArray());
                        }
                    }

                    // Create ICO file
                    var outputIcoPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "resources", "nl", "siwoc", "RouteManager", "ui", "app.ico");
                    using (var stream2 = File.Create(outputIcoPath))
                    using (var writer = new BinaryWriter(stream2))
                    {
                        // Write ICO header
                        writer.Write((short)0);    // Reserved
                        writer.Write((short)1);    // Type: ICO
                        writer.Write((short)sizes.Length);  // Number of images

                        // Calculate initial offset for image data
                        int offset = 6 + (sizes.Length * 16);  // Header + (number of images * directory entry size)

                        // Write directory entries
                        for (int i = 0; i < sizes.Length; i++)
                        {
                            writer.Write((byte)sizes[i]);      // Width
                            writer.Write((byte)sizes[i]);      // Height
                            writer.Write((byte)0);             // Color palette
                            writer.Write((byte)0);             // Reserved
                            writer.Write((short)1);            // Color planes
                            writer.Write((short)32);           // Bits per pixel
                            writer.Write(imageData[i].Length); // Size of image data
                            writer.Write(offset);              // Offset to image data
                            offset += imageData[i].Length;     // Update offset for next image
                        }

                        // Write image data
                        foreach (var data in imageData)
                        {
                            writer.Write(data);
                        }
                    }

                    MessageBox.Show($"Multi-size icon created at: {outputIcoPath}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating icon: {ex.Message}\n\nStack trace: {ex.StackTrace}");
            }
            finally
            {
                Close();
            }
        }
    }
} 