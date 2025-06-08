using System.Drawing;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;

#pragma warning disable CA1416

namespace ELRCRobTool.Robberies;
public class ATM
{
    private const int StartTime = 2;
    
    private static Color BorderColor = ColorTranslator.FromHtml("#1B2A35");
    
    static int borderSizeX = (int)(822 * Screen.SystemScaleMultiplier);
    static int borderSizeY = (int)(548 * Screen.SystemScaleMultiplier);

    private static int centerX = 0, centerY = 0;
    private static int borderX = 0, borderY = 0;
    
    private static Color GetColorToFind()
    {
        if (Program.ShouldStop()) return Color.Black; // Thoát sớm nếu Ctrl+C

        int fromX = (centerX) + (int)(10 * Screen.SystemScaleMultiplier);
        int toX = fromX + (int)(210 * Screen.SystemScaleMultiplier);
        int fromY = borderY + (int)(80 * Screen.SystemScaleMultiplier);
        int toY = borderY + (int)(100 * Screen.SystemScaleMultiplier);
    
        Bitmap screen = Screen.TakeScreenshot();
        Color highestColor = Color.Black;

        for (int x = fromX; x < toX; x++)
        {
            for (int y = fromY; y < toY; y++)
            {
                if (Program.ShouldStop()) return Color.Black; // Thoát sớm nếu Ctrl+C
                Color pColor = screen.GetPixel(x, y);
                if (pColor.R > highestColor.R & pColor.G > highestColor.G & pColor.B > highestColor.B)
                {
                    highestColor = pColor;
                }
            }
        }

        return highestColor;
    }

    public static void StartProcess()
    {
        Console.WriteLine($"i ~ Starting process in {StartTime}");
        Roblox.FocusRoblox();

        Thread.Sleep(StartTime * 1000);

        (borderX, borderY) = Screen.LocateColor(BorderColor);
        if (borderX == 0 && borderY == 0)
        {
            Console.WriteLine("! ~ Could not find ATM Firewall's Frame!");
            Thread.Sleep(1000);
            return;
        }
        
        centerX = borderX + (borderSizeX / 2);
        centerY = borderY + (borderSizeY / 2);

        int fromX = borderX + 82;
        int toX = (borderX + borderSizeX) - 84;
        int fromY = centerY - 128;
        int toY = (borderY + borderSizeY) - 73;

        while (true)
        {
            if (Program.ShouldStop()) break;

            try
            {
                (borderX, borderY) = Screen.LocateColor(BorderColor);
                if (borderX == 0 && borderY == 0)
                {
                    Console.WriteLine("! ~ Could not find ATM Firewall's Frame!");
                    Thread.Sleep(1000);
                    break;
                }

                Color colorToFind = GetColorToFind();
                if (Program.ShouldStop()) break; // Thoát sớm nếu Ctrl+C

                var (foundColorPosX, foundColorPosY) = Screen.FindColorInArea(
                    colorToFind, colorToFind, 2,
                    fromX, toX, fromY, toY
                );

                if (foundColorPosX == 0 && foundColorPosY == 0)
                {
                    Console.WriteLine("! ~ Could not find Color position in ATM's Firewall Frame!");
                    break;
                }

                Console.WriteLine($"i ~ Color to detect : {colorToFind.R},{colorToFind.G},{colorToFind.B}");
                Mouse.SetMousePos(foundColorPosX, foundColorPosY);

                while (true)
                {
                    if (Program.ShouldStop()) return;

                    Color textColor = Screen.GetColorAtPixel(foundColorPosX, foundColorPosY);
                    if (!Screen.AreColorsClose(textColor, colorToFind, 5))
                    {
                        Mouse.LeftClick();
                        Console.WriteLine("i ~ Clicked...");
                        break;
                    }
                }

                Console.WriteLine("i ~ Switching to the new Color in 0.5 seconds...");
                Thread.Sleep(500);
            }
            catch (ArgumentOutOfRangeException Ex)
            {
                Console.WriteLine($"! ~ Exception during automation: {Ex.Message}\n\nDid you select the correct minigame?");
                return;
            }
        }

        Console.WriteLine("i ~ Robbing Finished!");
    }
}