using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Windows.Media.Imaging;

namespace PixelLab_Desktop.Helpers
{
    /// <summary>دوال تحويل الألوان باستخدام WriteableBitmap.</summary>
    public static class ColorSpaceConverter
    {
        public static WriteableBitmap ToCmyk(WriteableBitmap src)
        {
            int w = src.PixelWidth, h = src.PixelHeight, stride = w * 4;
            byte[] pixels = new byte[h * stride];
            src.CopyPixels(pixels, stride, 0);
            for (int y = 0; y < h; y++)
            {
                int row = y * stride;
                for (int x = 0; x < w; x++)
                {
                    int idx = row + x * 4;
                    byte b = pixels[idx], g = pixels[idx + 1], r = pixels[idx + 2];
                    int c = 255 - r, m = 255 - g, yk = 255 - b;
                    pixels[idx] = (byte)yk;
                    pixels[idx + 1] = (byte)m;
                    pixels[idx + 2] = (byte)c;
                }
            }
            WriteableBitmap dst = new WriteableBitmap(w, h, src.DpiX, src.DpiY, src.Format, null);
            dst.WritePixels(new System.Windows.Int32Rect(0, 0, w, h), pixels, stride, 0);
            return dst;
        }

        public static WriteableBitmap ToHsv(WriteableBitmap src)
        {
            int w = src.PixelWidth, h = src.PixelHeight, stride = w * 4;
            byte[] pixels = new byte[h * stride];
            src.CopyPixels(pixels, stride, 0);
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
                    byte hue = (byte)(hsv / 360f * 255);
                    byte sat = (byte)(s * 255);
                    byte val = (byte)(v * 255);
                    pixels[idx] = val;
                    pixels[idx + 1] = sat;
                    pixels[idx + 2] = hue;
                }
            }
            WriteableBitmap dst = new WriteableBitmap(w, h, src.DpiX, src.DpiY, src.Format, null);
            dst.WritePixels(new System.Windows.Int32Rect(0, 0, w, h), pixels, stride, 0);
            return dst;
        }

        public static WriteableBitmap ToYuv(WriteableBitmap src)
        {
            int w = src.PixelWidth, h = src.PixelHeight, stride = w * 4;
            byte[] pixels = new byte[h * stride];
            src.CopyPixels(pixels, stride, 0);
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
                    pixels[idx] = (byte)Math.Clamp(V, 0, 255);
                    pixels[idx + 1] = (byte)Math.Clamp(U, 0, 255);
                    pixels[idx + 2] = (byte)Math.Clamp(Y, 0, 255);
                }
            }
            WriteableBitmap dst = new WriteableBitmap(w, h, src.DpiX, src.DpiY, src.Format, null);
            dst.WritePixels(new System.Windows.Int32Rect(0, 0, w, h), pixels, stride, 0);
            return dst;
        }

        public static WriteableBitmap ToYCbCr(WriteableBitmap src)
        {
            int w = src.PixelWidth, h = src.PixelHeight, stride = w * 4;
            byte[] pixels = new byte[h * stride];
            src.CopyPixels(pixels, stride, 0);
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
                    pixels[idx] = (byte)Math.Clamp(Cr, 0, 255);
                    pixels[idx + 1] = (byte)Math.Clamp(Cb, 0, 255);
                    pixels[idx + 2] = (byte)Math.Clamp(Y, 0, 255);
                }
            }
            WriteableBitmap dst = new WriteableBitmap(w, h, src.DpiX, src.DpiY, src.Format, null);
            dst.WritePixels(new System.Windows.Int32Rect(0, 0, w, h), pixels, stride, 0);
            return dst;
        }

        public static WriteableBitmap ToLab(WriteableBitmap src)
        {
            int w = src.PixelWidth, h = src.PixelHeight, stride = w * 4;
            byte[] pixels = new byte[h * stride];
            src.CopyPixels(pixels, stride, 0);
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
                    float fx = (X / refX > 0.008856f) ? (float)Math.Pow(X / refX, 1 / 3f) : 7.787f * X / refX + 16 / 116f;
                    float fy = (Y / refY > 0.008856f) ? (float)Math.Pow(Y / refY, 1 / 3f) : 7.787f * Y / refY + 16 / 116f;
                    float fz = (Z / refZ > 0.008856f) ? (float)Math.Pow(Z / refZ, 1 / 3f) : 7.787f * Z / refZ + 16 / 116f;
                    float L = 116 * fy - 16;
                    float A = 500 * (fx - fy);
                    float B2 = 200 * (fy - fz);
                    byte Lb = (byte)(L / 100f * 255);
                    byte Ab = (byte)((A + 128) / 255f * 255);
                    byte Bb = (byte)((B2 + 128) / 255f * 255);
                    pixels[idx] = Bb; pixels[idx + 1] = Ab; pixels[idx + 2] = Lb;
                }
            }
            WriteableBitmap dst = new WriteableBitmap(w, h, src.DpiX, src.DpiY, src.Format, null);
            dst.WritePixels(new System.Windows.Int32Rect(0, 0, w, h), pixels, stride, 0);
            return dst;
        }
    }
}