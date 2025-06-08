using System.Drawing;
using System.Threading;

namespace ELRCRobTool.Robberies
{
    public static class GlassCutting
    {
        private const int StartTime = 1;

        private static readonly Color SquareColor = Color.FromArgb(255, 85, 255, 0);
        private static readonly Color SquareColor0 = Color.FromArgb(255, 255, 0, 0);

        private static readonly int OFFSET = (int)Math.Floor(23 * Screen.SystemScaleMultiplier);
        private static readonly int SEARCH_OFFSET = (int)Math.Floor(15 * Screen.SystemScaleMultiplier);

        public static void StartProcess()
        {
            Screen.ReleaseDC();
            Screen.Init();
            Logger.WriteLine($"i ~ Starting process in {StartTime}");
            Roblox.FocusRoblox();

            Thread.Sleep(StartTime * 1000);

            int screenWidth = Screen.ScreenWidth;
            int screenHeight = Screen.ScreenHeight;
            int h5 = screenHeight / 5;
            int w3 = screenWidth / 3;

            int left = w3;
            int right = screenWidth - w3;
            int top = h5;
            int bottom = screenHeight - h5;

            bool wasSquareFound = false;
            int findingAttempts = 0;
            int oldX = 0, oldY = 0;
            bool isMinigameDone = false;

            try
            {
                while (!isMinigameDone)
                {
                    if (!Roblox.IsRobloxFocused())
                    {
                        Logger.WriteLine("i ~ Roblox lost focus, stopping GlassCutting...");
                        break;
                    }

                    if (!Roblox.IsRobloxRunning()) break;

                    int x, y;
                    if (wasSquareFound)
                    {
                        (x, y) = Screen.FindColorInArea(
                            SquareColor, SquareColor0, 10,
                            oldX - SEARCH_OFFSET, oldX + SEARCH_OFFSET,
                            oldY - SEARCH_OFFSET, oldY + SEARCH_OFFSET);
                    }
                    else
                    {
                        (x, y) = Screen.FindColorInArea(
                            SquareColor, SquareColor0, 15,
                            left, right, top, bottom);
                    }

                    if (x == 0 && y == 0)
                    {
                        wasSquareFound = false;
                        if (++findingAttempts > 10)
                        {
                            isMinigameDone = true;
                            
                            Logger.WriteLine("i ~ Could not find square after 25 attempts, minigame done.");
                        }
                    }
                    else
                    {
                        findingAttempts = 0;
                        oldX = x;
                        oldY = y;

                        x += OFFSET;
                        y += OFFSET;

                        Mouse.SetMousePos(x, y);

                        if (!wasSquareFound)
                        {
                            Mouse.SetMousePos(x + 2, y + 2);
                            Mouse.SetMousePos(x - 2, y - 2);
                            wasSquareFound = true;
                        }
                    }
                }
            }
            finally
            {
                Mouse.SetMousePos(Screen.ScreenWidth / 2, Screen.ScreenHeight / 2); // Đặt chuột về giữa
                Thread.Sleep(100); // Đợi ổn định
                
                Logger.WriteLine("i ~ GlassCutting stopped, mouse fixed at center and DC released!");
            }
            Screen.ReleaseDC(); // Giải phóng DC
        }
    }
}