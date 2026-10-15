using System;
using System.IO;
using System.Text;

namespace HostTournament
{
    /// <summary>
    /// 文件日志，crash-safe，每次启动清空。
    /// 写入：C:\Users\{user}\Documents\Mount and Blade II Bannerlord\Logs\HostTournament.log
    /// </summary>
    public static class HostTournamentLogger
    {
        private static string _logPath;
        private static readonly object _lock = new object();
        private static bool _initialized;

        private static string ResolvePath()
        {
            string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dir = Path.Combine(docs, "Mount and Blade II Bannerlord", "Logs");
            try { Directory.CreateDirectory(dir); } catch { }
            return Path.Combine(dir, "HostTournament.log");
        }

        private static void EnsureInit()
        {
            if (_initialized) return;
            _logPath = ResolvePath();
            try
            {
                // 清空旧日志
                File.WriteAllText(_logPath, "=== HostTournament log started " +
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ===\r\n",
                    Encoding.UTF8);
            }
            catch { }
            _initialized = true;
        }

        public static void Log(string message)
        {
            try
            {
                lock (_lock)
                {
                    EnsureInit();
                    string line = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] " + message;
                    File.AppendAllText(_logPath, line + "\r\n", Encoding.UTF8);
                }
            }
            catch { }
        }

        public static void LogErr(string message)
        {
            Log("[ERROR] " + message);
        }

        public static void LogV(string message)
        {
            if (HostTournamentConfig.VerboseLog) Log("[V] " + message);
        }
    }
}
