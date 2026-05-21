using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using PixelLab_Desktop.Models;
using PixelLab_Desktop.Services;
using System.Windows.Media.Imaging;

namespace PixelLab_Desktop.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ImageService _imageService = new();

        // The currently loaded image model
        [ObservableProperty]
        private ImageModel? currentImage;

        // The bitmap shown in the View
        [ObservableProperty]
        private BitmapSource? displayedBitmap;

        // Command: open file dialog and load image
        [RelayCommand]
        private void LoadImage()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp"
            };

            if (dialog.ShowDialog() == true)
            {
                CurrentImage = new ImageModel { FilePath = dialog.FileName };
                DisplayedBitmap = _imageService.LoadImage(dialog.FileName);
            }
        }

        // Command: save current image back to disk
        [RelayCommand]
        private void SaveImage()
        {
            if (DisplayedBitmap is null || CurrentImage is null) return;

            var dialog = new SaveFileDialog
            {
                Filter = "PNG Image|*.png",
                FileName = CurrentImage.FileName
            };

            if (dialog.ShowDialog() == true)
            {
                _imageService.SaveImage(DisplayedBitmap, dialog.FileName);
            }
        }
    }
}
