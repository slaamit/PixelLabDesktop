using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using PixelLab_Desktop.Helpers;

namespace PixelLab_Desktop.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // الصورة الأصلية
        private WriteableBitmap? _originalWb;
        private BitmapImage? _currentImage;
        public BitmapImage? CurrentImage
        {
            get => _currentImage;
            set { _currentImage = value; OnPropertyChanged(); }
        }

        // معلومات الصورة (المتطلب الثامن)
        private string _imageInfo = "No image loaded.";
        public string ImageInfo
        {
            get => _imageInfo;
            set { _imageInfo = value; OnPropertyChanged(); }
        }

        // تخزين البيانات الأساسية للصورة
        private string _originalFileName = "";
        private string _originalFileSize = "";
        private string _originalImageFormat = "";
        private int _originalWidth, _originalHeight;

        // أوامر المتطلب الثاني
        public ICommand LoadImageCommand { get; }
        public ICommand SaveImageCommand { get; }
        public ICommand ConvertToCmykCommand { get; }
        public ICommand ConvertToHsvCommand { get; }
        public ICommand ConvertToYuvCommand { get; }
        public ICommand ConvertToYCbCrCommand { get; }
        public ICommand ConvertToLabCommand { get; }
        public ICommand ResetToOriginalCommand { get; }

        // --- قنوات RGB (المتطلب الثالث) ---
        private bool _redEnabled = true, _greenEnabled = true, _blueEnabled = true;
        public bool RedEnabled { get => _redEnabled; set { _redEnabled = value; ApplyRgbAdjustments(); OnPropertyChanged(); } }
        public bool GreenEnabled { get => _greenEnabled; set { _greenEnabled = value; ApplyRgbAdjustments(); OnPropertyChanged(); } }
        public bool BlueEnabled { get => _blueEnabled; set { _blueEnabled = value; ApplyRgbAdjustments(); OnPropertyChanged(); } }

        private byte _redValue = 255, _greenValue = 255, _blueValue = 255;
        public byte RedValue { get => _redValue; set { _redValue = value; ApplyRgbAdjustments(); OnPropertyChanged(); } }
        public byte GreenValue { get => _greenValue; set { _greenValue = value; ApplyRgbAdjustments(); OnPropertyChanged(); } }
        public byte BlueValue { get => _blueValue; set { _blueValue = value; ApplyRgbAdjustments(); OnPropertyChanged(); } }

        // --- قنوات CMYK ---
        private bool _cyanEnabled = true, _magentaEnabled = true, _yellowEnabled = true, _blackEnabled = true;
        public bool CyanEnabled { get => _cyanEnabled; set { _cyanEnabled = value; ApplyCmykAdjustments(); OnPropertyChanged(); } }
        public bool MagentaEnabled { get => _magentaEnabled; set { _magentaEnabled = value; ApplyCmykAdjustments(); OnPropertyChanged(); } }
        public bool YellowEnabled { get => _yellowEnabled; set { _yellowEnabled = value; ApplyCmykAdjustments(); OnPropertyChanged(); } }
        public bool BlackEnabled { get => _blackEnabled; set { _blackEnabled = value; ApplyCmykAdjustments(); OnPropertyChanged(); } }

        private byte _cyanValue = 255, _magentaValue = 255, _yellowValue = 255, _blackValue = 255;
        public byte CyanValue { get => _cyanValue; set { _cyanValue = value; ApplyCmykAdjustments(); OnPropertyChanged(); } }
        public byte MagentaValue { get => _magentaValue; set { _magentaValue = value; ApplyCmykAdjustments(); OnPropertyChanged(); } }
        public byte YellowValue { get => _yellowValue; set { _yellowValue = value; ApplyCmykAdjustments(); OnPropertyChanged(); } }
        public byte BlackValue { get => _blackValue; set { _blackValue = value; ApplyCmykAdjustments(); OnPropertyChanged(); } }

        // --- قنوات HSV ---
        private bool _hueEnabled = true, _saturationEnabled = true, _valueEnabled = true;
        public bool HueEnabled { get => _hueEnabled; set { _hueEnabled = value; ApplyHsvAdjustments(); OnPropertyChanged(); } }
        public bool SaturationEnabled { get => _saturationEnabled; set { _saturationEnabled = value; ApplyHsvAdjustments(); OnPropertyChanged(); } }
        public bool ValueEnabled { get => _valueEnabled; set { _valueEnabled = value; ApplyHsvAdjustments(); OnPropertyChanged(); } }

        private byte _hueValue = 255, _saturationValue = 255, _valueValue = 255;
        public byte HueValue { get => _hueValue; set { _hueValue = value; ApplyHsvAdjustments(); OnPropertyChanged(); } }
        public byte SaturationValue { get => _saturationValue; set { _saturationValue = value; ApplyHsvAdjustments(); OnPropertyChanged(); } }
        public byte ValueValue { get => _valueValue; set { _valueValue = value; ApplyHsvAdjustments(); OnPropertyChanged(); } }

        // --- قنوات YUV ---
        private bool _yEnabled = true, _uEnabled = true, _vEnabled = true;
        public bool YEnabled { get => _yEnabled; set { _yEnabled = value; ApplyYuvAdjustments(); OnPropertyChanged(); } }
        public bool UEnabled { get => _uEnabled; set { _uEnabled = value; ApplyYuvAdjustments(); OnPropertyChanged(); } }
        public bool VEnabled { get => _vEnabled; set { _vEnabled = value; ApplyYuvAdjustments(); OnPropertyChanged(); } }

        private byte _yValue = 255, _uValue = 255, _vValue = 255;
        public byte YValue { get => _yValue; set { _yValue = value; ApplyYuvAdjustments(); OnPropertyChanged(); } }
        public byte UValue { get => _uValue; set { _uValue = value; ApplyYuvAdjustments(); OnPropertyChanged(); } }
        public byte VValue { get => _vValue; set { _vValue = value; ApplyYuvAdjustments(); OnPropertyChanged(); } }

        // --- قنوات YCbCr ---
        private bool _ycbcrYEnabled = true, _cbEnabled = true, _crEnabled = true;
        public bool YCbCrYEnabled { get => _ycbcrYEnabled; set { _ycbcrYEnabled = value; ApplyYCbCrAdjustments(); OnPropertyChanged(); } }
        public bool CbEnabled { get => _cbEnabled; set { _cbEnabled = value; ApplyYCbCrAdjustments(); OnPropertyChanged(); } }
        public bool CrEnabled { get => _crEnabled; set { _crEnabled = value; ApplyYCbCrAdjustments(); OnPropertyChanged(); } }

        private byte _ycbcrYValue = 255, _cbValue = 255, _crValue = 255;
        public byte YCbCrYValue { get => _ycbcrYValue; set { _ycbcrYValue = value; ApplyYCbCrAdjustments(); OnPropertyChanged(); } }
        public byte CbValue { get => _cbValue; set { _cbValue = value; ApplyYCbCrAdjustments(); OnPropertyChanged(); } }
        public byte CrValue { get => _crValue; set { _crValue = value; ApplyYCbCrAdjustments(); OnPropertyChanged(); } }

        // --- قنوات LAB ---
        private bool _lEnabled = true, _aEnabled = true, _bEnabled = true;
        public bool LEnabled { get => _lEnabled; set { _lEnabled = value; ApplyLabAdjustments(); OnPropertyChanged(); } }
        public bool AEnabled { get => _aEnabled; set { _aEnabled = value; ApplyLabAdjustments(); OnPropertyChanged(); } }
        public bool BEnabled { get => _bEnabled; set { _bEnabled = value; ApplyLabAdjustments(); OnPropertyChanged(); } }

        private byte _lValue = 255, _aValue = 255, _bValue = 255;
        public byte LValue { get => _lValue; set { _lValue = value; ApplyLabAdjustments(); OnPropertyChanged(); } }
        public byte AValue { get => _aValue; set { _aValue = value; ApplyLabAdjustments(); OnPropertyChanged(); } }
        public byte BValue { get => _bValue; set { _bValue = value; ApplyLabAdjustments(); OnPropertyChanged(); } }

        // المُنشئ
        public MainViewModel()
        {
            LoadImageCommand = new RelayCommand(ExecuteLoadImage);
            SaveImageCommand = new RelayCommand(ExecuteSaveImage);
            ConvertToCmykCommand = new RelayCommand(ExecuteConvertToCmyk);
            ConvertToHsvCommand = new RelayCommand(ExecuteConvertToHsv);
            ConvertToYuvCommand = new RelayCommand(ExecuteConvertToYuv);
            ConvertToYCbCrCommand = new RelayCommand(ExecuteConvertToYCbCr);
            ConvertToLabCommand = new RelayCommand(ExecuteConvertToLab);
            ResetToOriginalCommand = new RelayCommand(ExecuteResetToOriginal);
        }

        // دالة عامة للسحب والإفلات
        public void LoadImageFromPath(string path) => LoadImage(path);

        // فتح مربع حوار واختيار صورة
        private void ExecuteLoadImage()
        {
            var dlg = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp" };
            if (dlg.ShowDialog() == true) LoadImage(dlg.FileName);
        }

        // تحميل الصورة وحفظ معلوماتها (المتطلب الثامن)
        private void LoadImage(string path)
        {
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(path);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                var wb = new WriteableBitmap(bmp);
                _originalWb = wb;
                CurrentImage = bmp;
                _originalWidth = bmp.PixelWidth;
                _originalHeight = bmp.PixelHeight;

                // استخراج المعلومات
                _originalFileName = System.IO.Path.GetFileName(path);
                FileInfo fi = new FileInfo(path);
                long bytes = fi.Length;
                _originalFileSize = bytes >= 1024 * 1024
                    ? $"{bytes / (1024.0 * 1024.0):F2} MB"
                    : $"{bytes / 1024.0:F2} KB";

                string ext = System.IO.Path.GetExtension(path).ToLower();
                _originalImageFormat = ext switch
                {
                    ".jpg" or ".jpeg" => "JPEG",
                    ".png" => "PNG",
                    ".bmp" => "BMP",
                    ".gif" => "GIF",
                    _ => ext.TrimStart('.')
                };

                ImageInfo = $"📄 {_originalFileName} | {_originalImageFormat} | {_originalFileSize} | 📐 {_originalWidth}x{_originalHeight}";
            }
            catch (Exception ex) { ImageInfo = $"Error: {ex.Message}"; }
        }

        // حفظ الصورة
        private void ExecuteSaveImage()
        {
            if (_originalWb == null) return;
            var dlg = new SaveFileDialog { Filter = "PNG Image|*.png", FileName = "output.png" };
            if (dlg.ShowDialog() == true)
            {
                using var stream = new FileStream(dlg.FileName, FileMode.Create);
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(_originalWb));
                encoder.Save(stream);
                ImageInfo = $"Saved to {dlg.FileName}";
            }
        }

        // --- دوال التحويل (المتطلب الثاني) مع إبقاء المعلومات ---
        private void ExecuteConvertToCmyk()
        {
            if (_originalWb == null) return;
            var converted = ColorSpaceConverter.ToCmyk(_originalWb);
            CurrentImage = ConvertWriteableToBitmapImage(converted);
            ImageInfo = $"📄 {_originalFileName} | {_originalImageFormat} | {_originalFileSize} | (Converted to CMYK)";
        }

        private void ExecuteConvertToHsv()
        {
            if (_originalWb == null) return;
            var converted = ColorSpaceConverter.ToHsv(_originalWb);
            CurrentImage = ConvertWriteableToBitmapImage(converted);
            ImageInfo = $"📄 {_originalFileName} | {_originalImageFormat} | {_originalFileSize} | (Converted to HSV)";
        }

        private void ExecuteConvertToYuv()
        {
            if (_originalWb == null) return;
            var converted = ColorSpaceConverter.ToYuv(_originalWb);
            CurrentImage = ConvertWriteableToBitmapImage(converted);
            ImageInfo = $"📄 {_originalFileName} | {_originalImageFormat} | {_originalFileSize} | (Converted to YUV)";
        }

        private void ExecuteConvertToYCbCr()
        {
            if (_originalWb == null) return;
            var converted = ColorSpaceConverter.ToYCbCr(_originalWb);
            CurrentImage = ConvertWriteableToBitmapImage(converted);
            ImageInfo = $"📄 {_originalFileName} | {_originalImageFormat} | {_originalFileSize} | (Converted to YCbCr)";
        }

        private void ExecuteConvertToLab()
        {
            if (_originalWb == null) return;
            var converted = ColorSpaceConverter.ToLab(_originalWb);
            CurrentImage = ConvertWriteableToBitmapImage(converted);
            ImageInfo = $"📄 {_originalFileName} | {_originalImageFormat} | {_originalFileSize} | (Converted to LAB)";
        }

        private void ExecuteResetToOriginal()
        {
            if (_originalWb == null) return;
            CurrentImage = ConvertWriteableToBitmapImage(_originalWb);
            ImageInfo = $"📄 {_originalFileName} | {_originalImageFormat} | {_originalFileSize} | 📐 {_originalWidth}x{_originalHeight}";
        }

        // --- دوال التعديل الفوري (المتطلب الثالث) ---
        private void ApplyRgbAdjustments()
        {
            if (_originalWb == null) return;
            int w = _originalWb.PixelWidth, h = _originalWb.PixelHeight, stride = w * 4;
            byte[] pixels = new byte[h * stride];
            _originalWb.CopyPixels(pixels, stride, 0);
            for (int y = 0; y < h; y++)
            {
                int row = y * stride;
                for (int x = 0; x < w; x++)
                {
                    int idx = row + x * 4;
                    byte b = pixels[idx], g = pixels[idx + 1], r = pixels[idx + 2];
                    if (!RedEnabled) r = 0; else r = (byte)(r * RedValue / 255);
                    if (!GreenEnabled) g = 0; else g = (byte)(g * GreenValue / 255);
                    if (!BlueEnabled) b = 0; else b = (byte)(b * BlueValue / 255);
                    pixels[idx] = b; pixels[idx + 1] = g; pixels[idx + 2] = r;
                }
            }
            UpdateDisplay(w, h, stride, pixels);
        }

        private void ApplyCmykAdjustments()
        {
            if (_originalWb == null) return;
            int w = _originalWb.PixelWidth, h = _originalWb.PixelHeight, stride = w * 4;
            byte[] pixels = new byte[h * stride];
            _originalWb.CopyPixels(pixels, stride, 0);
            for (int y = 0; y < h; y++)
            {
                int row = y * stride;
                for (int x = 0; x < w; x++)
                {
                    int idx = row + x * 4;
                    byte b = pixels[idx], g = pixels[idx + 1], r = pixels[idx + 2];
                    int c = 255 - r, m = 255 - g, yk = 255 - b;
                    int k = Math.Min(c, Math.Min(m, yk));
                    if (!CyanEnabled) c = 0; else c = c * CyanValue / 255;
                    if (!MagentaEnabled) m = 0; else m = m * MagentaValue / 255;
                    if (!YellowEnabled) yk = 0; else yk = yk * YellowValue / 255;
                    if (!BlackEnabled) k = 0; else k = k * BlackValue / 255;
                    r = (byte)(255 - c - k); if (r < 0) r = 0;
                    g = (byte)(255 - m - k); if (g < 0) g = 0;
                    b = (byte)(255 - yk - k); if (b < 0) b = 0;
                    pixels[idx] = b; pixels[idx + 1] = g; pixels[idx + 2] = r;
                }
            }
            UpdateDisplay(w, h, stride, pixels);
        }

        private void ApplyHsvAdjustments()
        {
            if (_originalWb == null) return;
            int w = _originalWb.PixelWidth, h = _originalWb.PixelHeight, stride = w * 4;
            byte[] pixels = new byte[h * stride];
            _originalWb.CopyPixels(pixels, stride, 0);
            for (int y = 0; y < h; y++)
            {
                int row = y * stride;
                for (int x = 0; x < w; x++)
                {
                    int idx = row + x * 4;
                    byte b = pixels[idx], g = pixels[idx + 1], r = pixels[idx + 2];
                    float rf = r / 255f, gf = g / 255f, bf = b / 255f;
                    float max = Math.Max(rf, Math.Max(gf, bf));
                    float min = Math.Min(rf, Math.Min(gf, bf));
                    float hsv = 0, s = 0, v = max;
                    if (max - min > 0.001f)
                    {
                        float delta = max - min;
                        s = delta / max;
                        if (max == rf) hsv = (gf - bf) / delta;
                        else if (max == gf) hsv = 2 + (bf - rf) / delta;
                        else hsv = 4 + (rf - gf) / delta;
                        hsv *= 60; if (hsv < 0) hsv += 360;
                    }
                    float hueNorm = hsv / 360f;
                    if (!HueEnabled) hueNorm = 0; else hueNorm = hueNorm * HueValue / 255f;
                    if (!SaturationEnabled) s = 0; else s = s * SaturationValue / 255f;
                    if (!ValueEnabled) v = 0; else v = v * ValueValue / 255f;
                    // HSV to RGB
                    int hi = (int)(hueNorm * 6) % 6;
                    float f = hueNorm * 6 - hi;
                    float p = v * (1 - s);
                    float q = v * (1 - f * s);
                    float t = v * (1 - (1 - f) * s);
                    float rr = 0, gg = 0, bb = 0;
                    switch (hi)
                    {
                        case 0: rr = v; gg = t; bb = p; break;
                        case 1: rr = q; gg = v; bb = p; break;
                        case 2: rr = p; gg = v; bb = t; break;
                        case 3: rr = p; gg = q; bb = v; break;
                        case 4: rr = t; gg = p; bb = v; break;
                        case 5: rr = v; gg = p; bb = q; break;
                    }
                    pixels[idx] = (byte)(bb * 255);
                    pixels[idx + 1] = (byte)(gg * 255);
                    pixels[idx + 2] = (byte)(rr * 255);
                }
            }
            UpdateDisplay(w, h, stride, pixels);
        }

        private void ApplyYuvAdjustments()
        {
            if (_originalWb == null) return;
            int w = _originalWb.PixelWidth, h = _originalWb.PixelHeight, stride = w * 4;
            byte[] pixels = new byte[h * stride];
            _originalWb.CopyPixels(pixels, stride, 0);
            for (int y = 0; y < h; y++)
            {
                int row = y * stride;
                for (int x = 0; x < w; x++)
                {
                    int idx = row + x * 4;
                    byte b = pixels[idx], g = pixels[idx + 1], r = pixels[idx + 2];
                    float Y = 0.299f * r + 0.587f * g + 0.114f * b;
                    float U = -0.14713f * r - 0.28886f * g + 0.436f * b + 128;
                    float V = 0.615f * r - 0.51499f * g - 0.10001f * b + 128;
                    if (!YEnabled) Y = 0; else Y = Y * YValue / 255f;
                    if (!UEnabled) U = 0; else U = U * UValue / 255f;
                    if (!VEnabled) V = 0; else V = V * VValue / 255f;
                    float rr = Y + 1.13983f * (V - 128);
                    float gg = Y - 0.39465f * (U - 128) - 0.58060f * (V - 128);
                    float bb = Y + 2.03211f * (U - 128);
                    pixels[idx] = (byte)Math.Clamp(bb, 0, 255);
                    pixels[idx + 1] = (byte)Math.Clamp(gg, 0, 255);
                    pixels[idx + 2] = (byte)Math.Clamp(rr, 0, 255);
                }
            }
            UpdateDisplay(w, h, stride, pixels);
        }

        private void ApplyYCbCrAdjustments()
        {
            if (_originalWb == null) return;
            int w = _originalWb.PixelWidth, h = _originalWb.PixelHeight, stride = w * 4;
            byte[] pixels = new byte[h * stride];
            _originalWb.CopyPixels(pixels, stride, 0);
            for (int y = 0; y < h; y++)
            {
                int row = y * stride;
                for (int x = 0; x < w; x++)
                {
                    int idx = row + x * 4;
                    byte b = pixels[idx], g = pixels[idx + 1], r = pixels[idx + 2];
                    float Y = 0.299f * r + 0.587f * g + 0.114f * b;
                    float Cb = -0.168736f * r - 0.331264f * g + 0.5f * b + 128;
                    float Cr = 0.5f * r - 0.418688f * g - 0.081312f * b + 128;
                    if (!YCbCrYEnabled) Y = 0; else Y = Y * YCbCrYValue / 255f;
                    if (!CbEnabled) Cb = 0; else Cb = Cb * CbValue / 255f;
                    if (!CrEnabled) Cr = 0; else Cr = Cr * CrValue / 255f;
                    float rr = Y + 1.402f * (Cr - 128);
                    float gg = Y - 0.344136f * (Cb - 128) - 0.714136f * (Cr - 128);
                    float bb = Y + 1.772f * (Cb - 128);
                    pixels[idx] = (byte)Math.Clamp(bb, 0, 255);
                    pixels[idx + 1] = (byte)Math.Clamp(gg, 0, 255);
                    pixels[idx + 2] = (byte)Math.Clamp(rr, 0, 255);
                }
            }
            UpdateDisplay(w, h, stride, pixels);
        }

        private void ApplyLabAdjustments()
        {
            if (_originalWb == null) return;
            int w = _originalWb.PixelWidth, h = _originalWb.PixelHeight, stride = w * 4;
            byte[] pixels = new byte[h * stride];
            _originalWb.CopyPixels(pixels, stride, 0);
            for (int y = 0; y < h; y++)
            {
                int row = y * stride;
                for (int x = 0; x < w; x++)
                {
                    int idx = row + x * 4;
                    byte b = pixels[idx], g = pixels[idx + 1], r = pixels[idx + 2];
                    float R = r / 255f, G = g / 255f, B = b / 255f;
                    R = (R > 0.04045f) ? (float)Math.Pow((R + 0.055) / 1.055, 2.4) : R / 12.92f;
                    G = (G > 0.04045f) ? (float)Math.Pow((G + 0.055) / 1.055, 2.4) : G / 12.92f;
                    B = (B > 0.04045f) ? (float)Math.Pow((B + 0.055) / 1.055, 2.4) : B / 12.92f;
                    float X = R * 0.4124564f + G * 0.3575761f + B * 0.1804375f;
                    float Y = R * 0.2126729f + G * 0.7151522f + B * 0.0721750f;
                    float Z = R * 0.0193339f + G * 0.1191920f + B * 0.9503041f;
                    float refX = 0.95047f, refY = 1.0f, refZ = 1.08883f;
                    float fx = (X / refX > 0.008856f) ? (float)Math.Pow(X / refX, 1 / 3f) : (7.787f * X / refX + 16 / 116f);
                    float fy = (Y / refY > 0.008856f) ? (float)Math.Pow(Y / refY, 1 / 3f) : (7.787f * Y / refY + 16 / 116f);
                    float fz = (Z / refZ > 0.008856f) ? (float)Math.Pow(Z / refZ, 1 / 3f) : (7.787f * Z / refZ + 16 / 116f);
                    float L = 116 * fy - 16;
                    float A = 500 * (fx - fy);
                    float B2 = 200 * (fy - fz);
                    if (!LEnabled) L = 0; else L = L * LValue / 255f;
                    if (!AEnabled) A = 0; else A = A * AValue / 255f;
                    if (!BEnabled) B2 = 0; else B2 = B2 * BValue / 255f;
                    float fy2 = (L + 16) / 116f;
                    float fx2 = fy2 + A / 500f;
                    float fz2 = fy2 - B2 / 200f;
                    float x2 = (fx2 > 0.206893f) ? fx2 * fx2 * fx2 : (fx2 - 16 / 116f) / 7.787f;
                    float y2 = (fy2 > 0.206893f) ? fy2 * fy2 * fy2 : (fy2 - 16 / 116f) / 7.787f;
                    float z2 = (fz2 > 0.206893f) ? fz2 * fz2 * fz2 : (fz2 - 16 / 116f) / 7.787f;
                    X = x2 * refX; Y = y2 * refY; Z = z2 * refZ;
                    float Rl = (float)(X * 3.2404542 - Y * 1.5371385 - Z * 0.4985314);
                    float Gl = (float)(-X * 0.9692660 + Y * 1.8760108 + Z * 0.0415560);
                    float Bl = (float)(X * 0.0556434 - Y * 0.2040259 + Z * 1.0572252);
                    Rl = (Rl > 0.0031308f) ? (float)(1.055 * Math.Pow(Rl, 1 / 2.4) - 0.055) : 12.92f * Rl;
                    Gl = (Gl > 0.0031308f) ? (float)(1.055 * Math.Pow(Gl, 1 / 2.4) - 0.055) : 12.92f * Gl;
                    Bl = (Bl > 0.0031308f) ? (float)(1.055 * Math.Pow(Bl, 1 / 2.4) - 0.055) : 12.92f * Bl;
                    pixels[idx] = (byte)Math.Clamp(Bl * 255, 0, 255);
                    pixels[idx + 1] = (byte)Math.Clamp(Gl * 255, 0, 255);
                    pixels[idx + 2] = (byte)Math.Clamp(Rl * 255, 0, 255);
                }
            }
            UpdateDisplay(w, h, stride, pixels);
        }

        // تحديث الصورة المعروضة بعد تعديل القنوات
        private void UpdateDisplay(int w, int h, int stride, byte[] pixels)
        {
            WriteableBitmap result = new WriteableBitmap(w, h, _originalWb.DpiX, _originalWb.DpiY, _originalWb.Format, null);
            result.WritePixels(new System.Windows.Int32Rect(0, 0, w, h), pixels, stride, 0);
            CurrentImage = ConvertWriteableToBitmapImage(result);
        }

        // تحويل WriteableBitmap إلى BitmapImage (للعرض في WPF)
        private BitmapImage ConvertWriteableToBitmapImage(WriteableBitmap wb)
        {
            using var ms = new MemoryStream();
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(wb));
            encoder.Save(ms);
            ms.Position = 0;
            var result = new BitmapImage();
            result.BeginInit();
            result.StreamSource = ms;
            result.CacheOption = BitmapCacheOption.OnLoad;
            result.EndInit();
            result.Freeze();
            return result;
        }

        // تنفيذ INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}