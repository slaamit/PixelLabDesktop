using System.IO;
using System.Windows.Media.Imaging;

namespace PixelLab_Desktop.Services
{
    /// <summary>
    /// DEV_NOTES : this class is just to reduce the noise code and let our code base be as pristine as it comes . 
    ///             and in here we'll add the functions that we only need to call and no need to go into it's details . 
    /// </summary>
    public class ImageService
    {
        public BitmapImage LoadImage(string filePath)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }

        public void SaveImage(BitmapSource bitmap, string filePath)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = new FileStream(filePath, FileMode.Create);
            encoder.Save(stream);
        }
    }
}
