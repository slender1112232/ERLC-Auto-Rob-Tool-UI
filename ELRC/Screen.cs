using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

#pragma warning disable CA1416

namespace ELRCRobTool
{
    public static class Screen
    {
        private const int DesktopHorzres = 118;
        private const int DesktopVertres = 117;

        public static int ScreenWidth;
        public static int ScreenHeight;

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll", EntryPoint = "GetDeviceCaps")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("gdi32.dll")]
        static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        static Screen()
        {
            IntPtr desktopDc = GetDC(GetDesktopWindow());
            ScreenWidth = GetDeviceCaps(desktopDc, DesktopHorzres);
            ScreenHeight = GetDeviceCaps(desktopDc, DesktopVertres);
        }

        public static Bitmap TakeScreenshot()
        {
            Bitmap nBitmap = new Bitmap(ScreenWidth, ScreenHeight);
            Graphics.FromImage(nBitmap).CopyFromScreen(0, 0, 0, 0, nBitmap.Size);
            return nBitmap;
        }

        public static Bitmap TakeScreenshot(int fromX, int toX, int fromY, int toY)
        {
            int width = toX - fromX;
            int height = toY - fromY;
            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(fromX, fromY, 0, 0, new Size(width, height));
            }
            return bitmap;
        }

        public static (int, int) LocateColor(Color color, int tolerance = 0)
        {
            using (Bitmap screen = TakeScreenshot(0, ScreenWidth, 0, ScreenHeight))
            {
                BitmapData data = screen.LockBits(new Rectangle(0, 0, screen.Width, screen.Height),
                    ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                try
                {
                    unsafe
                    {
                        byte* ptr = (byte*)data.Scan0;
                        for (int y = 0; y < data.Height; y++)
                        {
                            for (int x = 0; x < data.Width; x++)
                            {
                                if (Program.ShouldStop()) return (0, 0);
                                int index = y * data.Stride + x * 3;
                                int b = ptr[index];
                                int g = ptr[index + 1];
                                int r = ptr[index + 2];
                                Color pColor = Color.FromArgb(255, r, g, b);
                                if (pColor == color || AreColorsClose(pColor, color, tolerance))
                                {
                                    return (x, y);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    screen.UnlockBits(data);
                }
            }
            return (0, 0);
        }

        public static (int, int) FindColorInArea(Color color1, Color color2, int tolerance, int fromX, int toX, int fromY, int toY)
        {
            using (Bitmap screen = TakeScreenshot(fromX, toX, fromY, toY))
            {
                BitmapData data = screen.LockBits(new Rectangle(0, 0, screen.Width, screen.Height),
                    ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                try
                {
                    unsafe
                    {
                        byte* ptr = (byte*)data.Scan0;
                        for (int y = 0; y < data.Height; y++)
                        {
                            for (int x = 0; x < data.Width; x++)
                            {
                                if (Program.ShouldStop()) return (0, 0);
                                int index = y * data.Stride + x * 3;
                                int b = ptr[index];
                                int g = ptr[index + 1];
                                int r = ptr[index + 2];
                                Color pColor = Color.FromArgb(255, r, g, b);
                                if (pColor == color1 || pColor == color2 ||
                                    AreColorsClose(pColor, color1, tolerance) ||
                                    AreColorsClose(pColor, color2, tolerance))
                                {
                                    return (fromX + x, fromY + y);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    screen.UnlockBits(data);
                }
            }
            return (0, 0);
        }

        public static Color GetColorAtPixel(int x, int y)
        {
            IntPtr hdc = GetDC(IntPtr.Zero);
            uint pixel = GetPixel(hdc, x, y);
            ReleaseDC(IntPtr.Zero, hdc);
            return Color.FromArgb(255,
                (int)(pixel & 0xFF),
                (int)((pixel & 0xFF00) >> 8),
                (int)((pixel & 0xFF0000) >> 16)
            );
        }

        public static bool AreColorsClose(Color color1, Color color2, int maxDiff)
        {
            return Math.Abs(color1.R - color2.R) <= maxDiff &&
                   Math.Abs(color1.G - color2.G) <= maxDiff &&
                   Math.Abs(color1.B - color2.B) <= maxDiff;
        }

        public static double GetScale()
        {
            try
            {
                var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop\WindowMetrics");
                if (key != null)
                {
                    object? scaleValue = key.GetValue("AppliedDPI");
                    if (scaleValue != null)
                    {
                        int dpi = Convert.ToInt32(scaleValue);
                        double scale = 1;
                        if (dpi != 96)
                        {
                            scale = (dpi / 96f);
                        }
                        return scale;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            return 1;
        }

        public static double SystemScaleMultiplier = Screen.GetScale();
    }
}