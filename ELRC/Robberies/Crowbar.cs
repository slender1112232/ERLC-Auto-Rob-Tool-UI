using System.Drawing;

namespace ELRCRobTool.Robberies;
public class Crowbar
{
    private const int StartTime = 1;
    private static Color GreenLineColor = ColorTranslator.FromHtml("#88D415");
    
    public static void StartProcess()
    {
        Console.WriteLine($"i ~ Starting process in {StartTime}");
        Roblox.FocusRoblox();
        
        Thread.Sleep(StartTime * 1000);

        while (true)
        {
            if (Program.ShouldStop()) break; // Giữ kiểm tra cơ bản nhưng không spam

            var (greenLineX, greenLineY) = Screen.LocateColor(GreenLineColor, 5);
            if (greenLineX == 0)
            {
                Console.WriteLine("! ~ Could not find Crowbar green line!");
                break;
            }

            Color oldColor = Screen.GetColorAtPixel(greenLineX + 7, greenLineY);
            int xOffset = greenLineX + 7;
            
            Mouse.SetMousePos(xOffset, greenLineY);

            while (true)
            {
                if (Program.ShouldStop()) break;

                Color currPixel = Screen.GetColorAtPixel(xOffset, greenLineY);
                if (!Screen.AreColorsClose(currPixel, oldColor, 5))
                {
                    Mouse.LeftClick();
                    Console.WriteLine("i ~ Clicked...");
                    Thread.Sleep(150);
                    break;
                }
            }
        }
        
        Console.WriteLine("i ~ Robbing Finished!");
    }
}