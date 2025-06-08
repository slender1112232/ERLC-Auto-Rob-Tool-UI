using System.Drawing;

namespace ELRCRobTool.Robberies;
public class LockPicking
{
    private const int StartTime = 1;
    private const int SpamClickCount = 5;
    private static Color LineColor = ColorTranslator.FromHtml("#FFC903");

    private static void SpamClickToFail(int x, int y)
    {
        Console.WriteLine("i ~ Spamming clicks to fail minigame...");
        for (int i = 0; i < SpamClickCount; i++)
        {
            Mouse.SetMousePos(x, y);
            Mouse.LeftClick();
            Thread.Sleep(50);
        }
        Console.WriteLine("i ~ Spam complete, minigame should fail now!");
    }

    public static void StartProcess()
    {
        Console.WriteLine($"i ~ Starting process in {StartTime}");
        Roblox.FocusRoblox();

        Thread.Sleep(StartTime * 1000);

        int barSizeOffset = (int)Math.Floor(83 * Screen.SystemScaleMultiplier);

        var (linePosX, linePosY) = Screen.LocateColor(LineColor, 0);
        if (linePosX == 0 && linePosY == 0)
        {
            Console.WriteLine("! ~ LockPicking line could not be found!");
            return;
        }

        Console.WriteLine($"i ~ Found Line at {linePosX}, {linePosY}");

        for (int rectI = 1; rectI < 7; rectI++)
        {
            int x = linePosX + (barSizeOffset * rectI);
            while (true)
            {
                if (Program.ShouldStop())
                {
                    SpamClickToFail(x, linePosY);
                    return;
                }

                Color color1 = Screen.GetColorAtPixel(x, linePosY + 10);
                Color color2 = Screen.GetColorAtPixel(x, linePosY - 4);

                if (
                    color1.R > 140 & color1.G > 140 & color1.B > 140 &&
                    color2.R > 140 & color2.G > 140 & color2.B > 140
                )
                {
                    Mouse.LeftClick();
                    Mouse.SetMousePos(x, linePosY);
                    Console.WriteLine("Switching to next bar");
                    Thread.Sleep(110);
                    break;
                }
            }
        }

        Console.WriteLine("i ~ Robbing Finished!");
    }
}