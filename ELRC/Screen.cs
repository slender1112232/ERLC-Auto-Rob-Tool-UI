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
        static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll", EntryPoint = "GetDeviceCaps")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("gdi32.dll")]
        static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        // Biến DC không còn static cố định, sẽ được lấy mới mỗi lần cần
        private static IntPtr _currentDC = IntPtr.Zero;

        // Lấy DC mới trước khi sử dụng
        private static void EnsureDC()
        {
            if (_currentDC == IntPtr.Zero)
            {
                _currentDC = GetDC(IntPtr.Zero);
                ScreenWidth = GetDeviceCaps(_currentDC, DesktopHorzres);
                ScreenHeight = GetDeviceCaps(_currentDC, DesktopVertres);
                Logger.WriteLine("i ~ New DC acquired.");
            }
        }
        public static void ReleaseDC()
        {
            if (_currentDC != IntPtr.Zero)
            {
                Logger.WriteLine("i ~ Releasing DC...");
                ReleaseDC(IntPtr.Zero, _currentDC);
                _currentDC = IntPtr.Zero;
                Logger.WriteLine("i ~ DC released.");
            }
        }

        public static Bitmap TakeScreenshot()
        {
            EnsureDC();
            Bitmap nBitmap = new Bitmap(ScreenWidth, ScreenHeight);
            Graphics.FromImage(nBitmap).CopyFromScreen(0, 0, 0, 0, nBitmap.Size);
            return nBitmap;
        }

        public static Bitmap TakeScreenshot(int fromX, int toX, int fromY, int toY)
        {
            EnsureDC();
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
            EnsureDC();
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
            EnsureDC();
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

        public static Color GetColorAtPixelFast(int x, int y)
        {
            EnsureDC();
            uint pixel = GetPixel(_currentDC, x, y);
            return Color.FromArgb(255,
                (int)(pixel & 0xFF),
                (int)((pixel >> 8) & 0xFF),
                (int)((pixel >> 16) & 0xFF));
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

        public static double SystemScaleMultiplier = GetScale();
    }
}