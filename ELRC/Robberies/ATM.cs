using System.Drawing;
using System.Threading;

#pragma warning disable CA1416

namespace ELRCRobTool.Robberies;
public class ATM
{
    private const int StartTime = 2;

    private static readonly Color BorderColor = ColorTranslator.FromHtml("#1B2A35");

    private static readonly int borderSizeX = (int)(822 * Screen.SystemScaleMultiplier);
    private static readonly int borderSizeY = (int)(548 * Screen.SystemScaleMultiplier);

    private static int centerX, centerY, borderX, borderY;

    private static Color GetColorToFind()
    {
        if (Program.ShouldStop()) return Color.Black;

        int fromX = centerX + (int)(10 * Screen.SystemScaleMultiplier);
        int toX = fromX + (int)(210 * Screen.SystemScaleMultiplier);
        int fromY = borderY + (int)(80 * Screen.SystemScaleMultiplier);
        int toY = borderY + (int)(100 * Screen.SystemScaleMultiplier);

        using Bitmap screen = Screen.TakeScreenshot();
        Color highest = Color.Black;

        for (int x = fromX; x < toX; x++)
        {
            for (int y = fromY; y < toY; y++)
            {
                if (Program.ShouldStop()) return Color.Black;
                Color p = screen.GetPixel(x, y);
                if (p.R > highest.R && p.G > highest.G && p.B > highest.B)
                    highest = p;
            }
        }
        return highest;
    }

    public static void StartProcess()
    {
        Screen.ReleaseDC();
        Screen.Init();

        Logger.WriteLine($"i ~ Starting process in {StartTime}");
        Roblox.FocusRoblox();

        Thread.Sleep(StartTime * 1000);

        (borderX, borderY) = Screen.LocateColor(BorderColor);
        if (borderX == 0 && borderY == 0)
        {
            Logger.WriteLine("! ~ Could not find ATM Firewall's Frame!");
            return;
        }

        centerX = borderX + borderSizeX / 2;
        centerY = borderY + borderSizeY / 2;

        int fromX = borderX + 82;
        int toX = borderX + borderSizeX - 84;
        int fromY = centerY - 128;
        int toY = borderY + borderSizeY - 73;

        while (true)
        {
            if (Program.ShouldStop()) break;

            (borderX, borderY) = Screen.LocateColor(BorderColor);
            if (borderX == 0 && borderY == 0)
            {
                Logger.WriteLine("! ~ Could not find ATM Firewall's Frame!");
                break;
            }

            Color targetColor = GetColorToFind();
            if (Program.ShouldStop()) break;

            var (px, py) = Screen.FindColorInArea(
                targetColor, targetColor, 2,
                fromX, toX, fromY, toY);

            if (px == 0 && py == 0)
            {
                Logger.WriteLine("! ~ Could not find color in ATM frame!");
                break;
            }

            Logger.WriteLine($"i ~ Color to detect: {targetColor.R},{targetColor.G},{targetColor.B}");
            Mouse.SetMousePos(px, py);

            while (true)
            {
                if (Program.ShouldStop()) return;

                Color now = Screen.GetColorAtPixelFast(px, py);
                if (!Screen.AreColorsClose(now, targetColor, 5))
                {
                    Mouse.LeftClick();
                    Logger.WriteLine("i ~ Clicked...");
                    break;
                }
            }

            Logger.WriteLine("i ~ Switching to new color in 0.5 s...");
            Thread.Sleep(500);
        }
        Screen.ReleaseDC();
        Logger.WriteLine("i ~ Robbing Finished and DC released!");
    }
}
