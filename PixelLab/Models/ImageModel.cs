using System.IO;

namespace PixelLab_Desktop.Models
{
    /// <summary>
    /// DEV_NOTES : this model is just to treat the image as a model that we can call later and implement the MVVM structure . 
    /// 
    /// NOTE : DO NOT CHANGE THIS MODEL AT ANY COST PLEASE PLEASE PLEASE PEOPLE , JUST LEAVE THIS AS IS PLEASE . 
    /// </summary>
    public class ImageModel
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName => Path.GetFileName(FilePath);
    }
}