using System.Drawing;
using System.Threading;

namespace ELRCRobTool.Robberies;
public class Crowbar
{
    private const int StartTime = 1;
    private static readonly Color GreenLineColor = ColorTranslator.FromHtml("#88D415");

    public static void StartProcess()
    {
        Screen.ReleaseDC();
        Screen.Init();

        Logger.WriteLine($"i ~ Starting process in {StartTime}");
        Roblox.FocusRoblox();

        Thread.Sleep(StartTime * 1000);

        while (true)
        {
            if (Program.ShouldStop()) break;

            var (greenLineX, greenLineY) = Screen.LocateColor(GreenLineColor, 5);
            if (greenLineX == 0)
            {
                Logger.WriteLine("! ~ Could not find Crowbar green line!");
                break;
            }

            int xOffset = greenLineX + 7;
            Color oldColor = Screen.GetColorAtPixelFast(xOffset, greenLineY);

            Mouse.SetMousePos(xOffset, greenLineY);

            while (true)
            {
                if (Program.ShouldStop()) break;

                Color currPixel = Screen.GetColorAtPixelFast(xOffset, greenLineY);
                if (!Screen.AreColorsClose(currPixel, oldColor, 5))
                {
                    Mouse.LeftClick();
                    Logger.WriteLine("i ~ Clicked...");
                    Thread.Sleep(150);
                    break;
                }
            }
        }

        // Thêm vào cuối StartProcess
        Screen.ReleaseDC();
        Logger.WriteLine("i ~ Robbing Finished and DC released!");
    }
}
