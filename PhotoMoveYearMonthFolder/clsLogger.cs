using System.Collections.Concurrent;
using System.Timers;

namespace PhotoMoveYearMonthFolder
{
    public static class Logger
    {
        private static readonly string logFilePath = "E:\\MyTemp\\OUT_TestIMG_log.txt";
        private static readonly ConcurrentQueue<string> logMessages = new();
        private static readonly System.Timers.Timer flushTimer;

        static Logger()
        {
            flushTimer = new System.Timers.Timer(10000); // Imposta l'intervallo a 10 secondi (10000 millisecondi)
            flushTimer.Elapsed += FlushLogToFile;
            flushTimer.AutoReset = true;
            flushTimer.Enabled = true;
        }

        public static void Log(string message)
        {
            logMessages.Enqueue($"{DateTime.Now}: {message}");
        }

        private static void FlushLogToFile(object source, ElapsedEventArgs e)
        {
            while (!logMessages.IsEmpty)
            {
                if (logMessages.TryDequeue(out string? logMessage))
                {
                    if (logMessage != null)
                    {
                        using StreamWriter writer = new(logFilePath, append: true);
                        writer.WriteLine(logMessage);
                    }
                }

            }
        }

    }

}
