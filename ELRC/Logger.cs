using System;
using System.IO;

namespace ELRCRobTool
{
    public static class Logger
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ERLC_Log.txt");
        private static readonly object Lock = new object();

        // Thêm event để thông báo khi có log mới
        public static event Action<string>? OnLogMessage;

        public static void WriteLine(string message)
        {
            string timestampedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            Console.WriteLine(timestampedMessage); // Vẫn in ra console
            lock (Lock) // Đảm bảo thread-safe khi ghi file
            {
                File.AppendAllText(LogFilePath, timestampedMessage + Environment.NewLine);
            }
            // Gửi thông điệp đến event để UI có thể cập nhật
            OnLogMessage?.Invoke(timestampedMessage);
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