using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace ELRCRobTool
{
    public class Roblox
    {
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

        public static Process? GetRbxProcess()
        {
            Process[] pArray = Process.GetProcessesByName("RobloxPlayerBeta");
            if (pArray.Length > 0)
            {
                return pArray[0];
            }

            return null;
        }

        public static bool IsRobloxRunning()
        {
            return GetRbxProcess() != null;
        }

        public static bool IsRobloxFocused()
        {
            IntPtr? robloxWindow = FindWindow(null, "Roblox");
            IntPtr? foregroundWindow = GetForegroundWindow();
            return robloxWindow != IntPtr.Zero && foregroundWindow != IntPtr.Zero && foregroundWindow == robloxWindow;
        }

        public static void FocusRoblox()
        {
            Process? RbxProcess = GetRbxProcess();
            if (RbxProcess != null)
            {
                Console.WriteLine("i ~ Focusing Roblox in 0.5 seconds");
                Thread.Sleep(500);

                SetForegroundWindow(RbxProcess.MainWindowHandle);
            }
        }
    }
}