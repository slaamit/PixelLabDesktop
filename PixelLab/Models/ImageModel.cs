using System.IO;

namespace PixelLab_Desktop.Models
{
    public class ImageModel
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName => Path.GetFileName(FilePath);
    }
}