using System.IO;
using System.Windows.Media.Imaging;

namespace PixelLab_Desktop.Services
{
    public class ImageService
    {
        // Load a BitmapImage from a file path
        public BitmapImage LoadImage(string filePath)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }

        // Save a BitmapSource to a file path (as PNG)
        public void SaveImage(BitmapSource bitmap, string filePath)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = new FileStream(filePath, FileMode.Create);
            encoder.Save(stream);
        }
    }
}
