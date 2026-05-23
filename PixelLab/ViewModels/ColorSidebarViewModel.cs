using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace PixelLab_Desktop.ViewModels
{
    /// <summary>
    /// this class is for the side bar in the project , when we choose a color from the 3D view we'll need to show the color and show it's values in all of the other color systems . 
    /// this class is a bit nested so please pay some attention for it . 
    /// </summary>
    public class ColorSidebarViewModel : INotifyPropertyChanged
    {
        // the color picked from the 3D space will be saved in this place . 
        // with a setter and getter to change it's values as needed ( so we don't get to change alot of things at the same time ) . 
        private Color _pickedColor = Colors.White;
        public Color PickedColor
        {
            get => _pickedColor;
            set
            {
                _pickedColor = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PickedBrush));
                RefreshAllSystems();
            }
        }

        public SolidColorBrush PickedBrush => new SolidColorBrush(_pickedColor);

        // this place will hold the text that is the values for all of the color systems . 
        // and will change on propertychanged function called to change the color to match the new one . 
        private string _rgbText  = "—";
        private string _hsvText  = "—";
        private string _cmykText = "—";
        private string _yuvText  = "—";
        private string _ycbcrText= "—";
        private string _labText  = "—";

        public string RgbText  { get => _rgbText;  private set { _rgbText  = value; OnPropertyChanged(); } }
        public string HsvText  { get => _hsvText;  private set { _hsvText  = value; OnPropertyChanged(); } }
        public string CmykText { get => _cmykText; private set { _cmykText = value; OnPropertyChanged(); } }
        public string YuvText  { get => _yuvText;  private set { _yuvText  = value; OnPropertyChanged(); } }
        public string YCbCrText{ get => _ycbcrText;private set { _ycbcrText= value; OnPropertyChanged(); } }
        public string LabText  { get => _labText;  private set { _labText  = value; OnPropertyChanged(); } }

        // the active space is the place to choose what tab should be showing in 3D spaces , with a default value for RGB . 
        private string _activeSpace = "RGB";
        public string ActiveSpace
        {
            get => _activeSpace;
            set { _activeSpace = value; OnPropertyChanged(); }
        }

        // DEV_NOTES :  in this function , we need to refresh the text and colors everytime a new color is picked , so we'll call this function . 
        //              but because we're reading from a pixel in a 3D view and that is on a screen , so we'll get an RGB value that would then be changed into the other color systems.
        private void RefreshAllSystems()
        {
            byte r = _pickedColor.R, g = _pickedColor.G, b = _pickedColor.B;

            RgbText = $"R={r}  G={g}  B={b}";

            // HSV
            float rf = r / 255f, gf = g / 255f, bf = b / 255f;
            float max = Math.Max(rf, Math.Max(gf, bf));
            float min = Math.Min(rf, Math.Min(gf, bf));
            float h = 0, s = 0, v = max;
            if (max - min > 0.001f)
            {
                float delta = max - min;
                s = delta / max;
                if (max == rf)      h = (gf - bf) / delta;
                else if (max == gf) h = 2 + (bf - rf) / delta;
                else                h = 4 + (rf - gf) / delta;
                h *= 60; if (h < 0) h += 360;
            }
            HsvText = $"H={h:F0}°  S={s * 100:F0}%  V={v * 100:F0}%";

            // CMYK
            float c2 = 1 - rf, m2 = 1 - gf, y2 = 1 - bf;
            float k2 = Math.Min(c2, Math.Min(m2, y2));
            if (k2 < 1) { c2 = (c2 - k2) / (1 - k2); m2 = (m2 - k2) / (1 - k2); y2 = (y2 - k2) / (1 - k2); }
            else { c2 = m2 = y2 = 0; }
            CmykText = $"C={c2 * 100:F0}%  M={m2 * 100:F0}%  Y={y2 * 100:F0}%  K={k2 * 100:F0}%";

            // YUV
            float Y  =  0.299f * r + 0.587f  * g + 0.114f   * b;
            float U  = -0.14713f * r - 0.28886f * g + 0.436f * b + 128;
            float V  =  0.615f  * r - 0.51499f * g - 0.10001f * b + 128;
            YuvText  = $"Y={Y:F0}  U={U:F0}  V={V:F0}";

            // YCbCr
            float Yc  =  0.299f    * r + 0.587f    * g + 0.114f    * b;
            float Cb  = -0.168736f * r - 0.331264f * g + 0.5f      * b + 128;
            float Cr  =  0.5f      * r - 0.418688f * g - 0.081312f * b + 128;
            YCbCrText = $"Y={Yc:F0}  Cb={Cb:F0}  Cr={Cr:F0}";

            // LAB
            float R2 = rf, G2 = gf, B2 = bf;
            R2 = (R2 > 0.04045f) ? (float)Math.Pow((R2 + 0.055) / 1.055, 2.4) : R2 / 12.92f;
            G2 = (G2 > 0.04045f) ? (float)Math.Pow((G2 + 0.055) / 1.055, 2.4) : G2 / 12.92f;
            B2 = (B2 > 0.04045f) ? (float)Math.Pow((B2 + 0.055) / 1.055, 2.4) : B2 / 12.92f;
            float X = R2 * 0.4124564f + G2 * 0.3575761f + B2 * 0.1804375f;
            float Yxyz = R2 * 0.2126729f + G2 * 0.7151522f + B2 * 0.0721750f;
            float Z = R2 * 0.0193339f + G2 * 0.1191920f + B2 * 0.9503041f;
            float fx = F(X / 0.95047f), fy = F(Yxyz / 1.0f), fz = F(Z / 1.08883f);
            float Llab = 116 * fy - 16;
            float Alab = 500 * (fx - fy);
            float Blab = 200 * (fy - fz);
            LabText = $"L={Llab:F1}  a={Alab:F1}  b={Blab:F1}";
        }

        private static float F(float t) =>
            t > 0.008856f ? (float)Math.Pow(t, 1 / 3f) : 7.787f * t + 16f / 116f;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
