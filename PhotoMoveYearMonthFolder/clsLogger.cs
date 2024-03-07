using System.Collections.Concurrent;
using System.Timers;

namespace PhotoMoveYearMonthFolder
{
    public static class Logger
    {        
        private static readonly ConcurrentQueue<string> logMessages = new();
        private static readonly System.Timers.Timer flushTimer;
        private static string logFilePath = "";

        static Logger()
        {
            flushTimer = new System.Timers.Timer(10000); // Imposta l'intervallo a 10 secondi (10000 millisecondi)
            flushTimer.Elapsed += FlushLogToFile;
            flushTimer.AutoReset = true;
            flushTimer.Enabled = true;
        }

        public static void SetLogFilePath(string path)
        {
            logFilePath = path; // Imposta il percorso ed il nome del file di log
        }

        public static void Log(string message)
        {
            logMessages.Enqueue($"{DateTime.Now}: {message}");
        }

        private static void FlushLogToFile(object? source, ElapsedEventArgs e)
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
