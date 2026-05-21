using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixelLab_Desktop.Helpers
{
    /// <summary>
    /// Generates 2D color space visualization bitmaps for CMYK, YUV, YCbCr, and LAB.
    /// Each method returns a WriteableBitmap you can display in a WPF Image control.
    /// </summary>
    public static class ColorSpaceVisualizer
    {
        // ─────────────────────────────────────────────
        // CMYK: 4 vertical gradient bars side by side
        // ─────────────────────────────────────────────
        public static WriteableBitmap DrawCmykBars(int width, int height)
        {
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];
            int barW = width / 4;

            for (int y = 0; y < height; y++)
            {
                // t goes from 0 (top = full channel) to 1 (bottom = zero channel)
                float t = 1f - (float)y / (height - 1);

                for (int x = 0; x < width; x++)
                {
                    int row = y * stride + x * 4;
                    byte r, g, b;

                    if (x < barW)
                    {
                        // Cyan bar: white → cyan
                        byte c = (byte)(t * 255);
                        r = (byte)(255 - c); g = 255; b = 255;
                    }
                    else if (x < barW * 2)
                    {
                        // Magenta bar: white → magenta
                        byte m = (byte)(t * 255);
                        r = 255; g = (byte)(255 - m); b = 255;
                    }
                    else if (x < barW * 3)
                    {
                        // Yellow bar: white → yellow
                        byte yk = (byte)(t * 255);
                        r = 255; g = 255; b = (byte)(255 - yk);
                    }
                    else
                    {
                        // Black bar: white → black
                        byte k = (byte)(t * 255);
                        r = (byte)(255 - k); g = (byte)(255 - k); b = (byte)(255 - k);
                    }

                    pixels[row]     = b;
                    pixels[row + 1] = g;
                    pixels[row + 2] = r;
                    pixels[row + 3] = 255;
                }
            }

            return MakeBitmap(width, height, stride, pixels);
        }

        // ─────────────────────────────────────────────
        // YUV: 2D chrominance plane (U horizontal, V vertical)
        // ─────────────────────────────────────────────
        public static WriteableBitmap DrawYuvPlane(int width, int height, float yNorm = 0.5f)
        {
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];
            float Y = yNorm * 255f;

            for (int py = 0; py < height; py++)
            {
                float V = (1f - (float)py / (height - 1)) * 255f - 128f; // -128..127
                for (int px = 0; px < width; px++)
                {
                    float U = ((float)px / (width - 1)) * 255f - 128f;   // -128..127

                    float r = Y + 1.13983f * V;
                    float g = Y - 0.39465f * U - 0.58060f * V;
                    float bv = Y + 2.03211f * U;

                    int row = py * stride + px * 4;
                    pixels[row]     = (byte)Math.Clamp(bv, 0, 255);
                    pixels[row + 1] = (byte)Math.Clamp(g,  0, 255);
                    pixels[row + 2] = (byte)Math.Clamp(r,  0, 255);
                    pixels[row + 3] = 255;
                }
            }

            return MakeBitmap(width, height, stride, pixels);
        }

        // ─────────────────────────────────────────────
        // YCbCr: 2D chrominance plane (Cb horizontal, Cr vertical)
        // ─────────────────────────────────────────────
        public static WriteableBitmap DrawYCbCrPlane(int width, int height, float yNorm = 0.5f)
        {
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];
            float Y = yNorm * 255f;

            for (int py = 0; py < height; py++)
            {
                float Cr = (1f - (float)py / (height - 1)) * 255f - 128f;
                for (int px = 0; px < width; px++)
                {
                    float Cb = ((float)px / (width - 1)) * 255f - 128f;

                    float r = Y + 1.402f * Cr;
                    float g = Y - 0.344136f * Cb - 0.714136f * Cr;
                    float bv = Y + 1.772f * Cb;

                    int row = py * stride + px * 4;
                    pixels[row]     = (byte)Math.Clamp(bv, 0, 255);
                    pixels[row + 1] = (byte)Math.Clamp(g,  0, 255);
                    pixels[row + 2] = (byte)Math.Clamp(r,  0, 255);
                    pixels[row + 3] = 255;
                }
            }

            return MakeBitmap(width, height, stride, pixels);
        }

        // ─────────────────────────────────────────────
        // LAB: 2D a*b* plane
        // ─────────────────────────────────────────────
        public static WriteableBitmap DrawLabPlane(int width, int height, float lNorm = 0.5f)
        {
            int stride = width * 4;
            byte[] pixels = new byte[height * stride];
            float L = lNorm * 100f; // L in 0..100

            for (int py = 0; py < height; py++)
            {
                float bStar = (1f - (float)py / (height - 1)) * 256f - 128f; // -128..128
                for (int px = 0; px < width; px++)
                {
                    float aStar = ((float)px / (width - 1)) * 256f - 128f;

                    // LAB → XYZ
                    float fy = (L + 16f) / 116f;
                    float fx = aStar / 500f + fy;
                    float fz = fy - bStar / 200f;

                    float refX = 0.95047f, refY = 1.0f, refZ = 1.08883f;
                    float x2 = (fx > 0.206893f) ? fx * fx * fx : (fx - 16f / 116f) / 7.787f;
                    float y2 = (fy > 0.206893f) ? fy * fy * fy : (fy - 16f / 116f) / 7.787f;
                    float z2 = (fz > 0.206893f) ? fz * fz * fz : (fz - 16f / 116f) / 7.787f;

                    float X = x2 * refX, Y2 = y2 * refY, Z = z2 * refZ;

                    // XYZ → RGB (linear)
                    float Rl = X * 3.2404542f - Y2 * 1.5371385f - Z * 0.4985314f;
                    float Gl = -X * 0.9692660f + Y2 * 1.8760108f + Z * 0.0415560f;
                    float Bl = X * 0.0556434f - Y2 * 0.2040259f + Z * 1.0572252f;

                    // Gamma
                    Rl = (Rl > 0.0031308f) ? (float)(1.055 * Math.Pow(Rl, 1 / 2.4) - 0.055) : 12.92f * Rl;
                    Gl = (Gl > 0.0031308f) ? (float)(1.055 * Math.Pow(Gl, 1 / 2.4) - 0.055) : 12.92f * Gl;
                    Bl = (Bl > 0.0031308f) ? (float)(1.055 * Math.Pow(Bl, 1 / 2.4) - 0.055) : 12.92f * Bl;

                    int row = py * stride + px * 4;
                    pixels[row]     = (byte)Math.Clamp(Bl * 255, 0, 255);
                    pixels[row + 1] = (byte)Math.Clamp(Gl * 255, 0, 255);
                    pixels[row + 2] = (byte)Math.Clamp(Rl * 255, 0, 255);
                    pixels[row + 3] = 255;
                }
            }

            return MakeBitmap(width, height, stride, pixels);
        }

        // ─────────────────────────────────────────────
        // Shared factory
        // ─────────────────────────────────────────────
        private static WriteableBitmap MakeBitmap(int w, int h, int stride, byte[] pixels)
        {
            var wb = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, null);
            wb.WritePixels(new Int32Rect(0, 0, w, h), pixels, stride, 0);
            return wb;
        }
    }
}
