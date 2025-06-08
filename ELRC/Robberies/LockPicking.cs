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


    private static bool IsWhite(Color c) =>
        c.R > whiteThr && c.G > whiteThr && c.B > whiteThr;

    private static bool WaitAndClick(int barIdx, int x, int lineY)
    {
        while (true)
        {
            if (Program.ShouldStop()) return false;

            foreach (int dy in yOffsets)
            {
                Color pix = Screen.GetColorAtPixelFast(x + 5, lineY + dy);
                if (IsWhite(pix))
                {
                    Mouse.LeftClick();
                    Logger.WriteLine($"i ~ Clicked bar {barIdx}");
                    Thread.Sleep(postClickWait);
                    return true;
                }
            }
            
            Thread.Sleep(1); // poll ~1 kHz
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

        // Thêm vào cuối StartProcess
        Screen.ReleaseDC();
        Logger.WriteLine("i ~ Robbing Finished and DC released!");
    }
}
