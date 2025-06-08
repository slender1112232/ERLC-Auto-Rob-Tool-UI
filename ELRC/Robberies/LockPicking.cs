using System.Drawing;
using System.Threading;

namespace ELRCRobTool.Robberies;
public class LockPicking
{
    private const int StartTime = 1;
    private const int SpamClickCount = 5;

    private static readonly Color LineColor = ColorTranslator.FromHtml("#FFC903");
    private static readonly int[] yOffsets = { -6, -4, -2, 0, 2, 4, 6, 8 }; // dải Y quét
    private const int whiteThr = 120;       // ngưỡng “trắng”
    private const int postClickWait = 85;        // ms nghỉ sau click

    /* ------------- helper ------------- */
    private static void SpamClickToFail(int x, int y)
    {
        Logger.WriteLine("i ~ Spamming clicks to fail minigame...");
        for (int i = 0; i < SpamClickCount; i++)
        {
            Mouse.SetMousePos(x, y);
            Mouse.LeftClick();
            Thread.Sleep(50);
        }
        Logger.WriteLine("i ~ Spam complete, minigame should fail now!");
    }

    private static bool IsWhite(Color c) =>
        c.R > whiteThr && c.G > whiteThr && c.B > whiteThr;

    private static bool WaitAndClick(int barIdx, int x, int lineY)
    {
        while (true)
        {
            if (Program.ShouldStop())
            {
                SpamClickToFail(x, lineY);
                return false;
            }

            bool hit = false;
            foreach (int dy in yOffsets)
            {
                Color pix = Screen.GetColorAtPixelFast(x + 5, lineY + dy);
                if (IsWhite(pix)) { hit = true; break; }
            }

            if (hit)
            {
                Mouse.LeftClick();
                Logger.WriteLine($"i ~ Clicked bar {barIdx}");
                Thread.Sleep(postClickWait);
                return true;
            }

            Thread.Sleep(1);          // poll ~1 kHz
        }
    }

    /* ------------- main ------------- */
    public static void StartProcess()
    {
        
        Logger.WriteLine($"i ~ Starting process in {StartTime}");
        Roblox.FocusRoblox();
        Thread.Sleep(StartTime * 5000);
        int barOffset = (int)Math.Floor(83 * Screen.SystemScaleMultiplier);

        var (lineX, lineY) = Screen.LocateColor(LineColor, 0);
        if (lineX == 0)
        {
            Logger.WriteLine("! ~ LockPicking line could not be found!");
            return;
        }
        Logger.WriteLine($"i ~ Found Line at {lineX}, {lineY}");
        
        for (int bar = 1; bar <= 6; bar++)
        {
            int x = lineX + barOffset * bar;
            Mouse.SetMousePos(x, lineY);
            Logger.WriteLine($"i ~ Moved to bar {bar} at {x}, {lineY}");
            if (!WaitAndClick(bar, x, lineY)) return;   // dừng nếu Ctrl+C
        }

        Logger.WriteLine("i ~ Robbing Finished!");
    }
}
