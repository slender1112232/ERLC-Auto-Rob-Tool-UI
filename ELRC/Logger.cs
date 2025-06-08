using System;
using System.IO;

namespace ELRCRobTool
{
    public static class Logger
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ERLC_Log.txt");
        private static readonly object Lock = new object();

        public static void WriteLine(string message)
        {
            string timestampedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            Console.WriteLine(timestampedMessage); // Vẫn in ra console
            lock (Lock) // Đảm bảo thread-safe khi ghi file
            {
                File.AppendAllText(LogFilePath, timestampedMessage + Environment.NewLine);
            }
        }

        public static void ClearLog()
        {
            lock (Lock)
            {
                if (File.Exists(LogFilePath))
                    File.WriteAllText(LogFilePath, string.Empty);
            }
        }
    }
}