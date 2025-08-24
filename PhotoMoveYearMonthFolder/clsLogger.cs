using System.Collections.Concurrent;
using System.Timers;

namespace PhotoMoveYearMonthFolder
{
    public static class Logger
    {
        private static readonly ConcurrentQueue<string> logMessages = new();
        private static readonly ConcurrentQueue<string> logErrorMessages = new();
        private static readonly System.Timers.Timer flushTimer;
        private static readonly object flushSync = new();
        private static string logFilePath = "";
        private static string logErrorFilePath = "";

        static Logger()
        {
            flushTimer = new System.Timers.Timer(10_000);
            flushTimer.AutoReset = true;
            flushTimer.Elapsed += (_, __) =>
            {
                try { FlushNow(); } catch { /* evita crash timer */ }
            };
            flushTimer.Enabled = true;
        }

        public static void SetLogFilePath(string path) => logFilePath = path;
        public static void SetErrorFilePath(string path) => logErrorFilePath = path;

        public static void Log(string message)
        {
            logMessages.Enqueue($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {message}");
        }

        public static void LogError(string message)
        {
            logErrorMessages.Enqueue($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {message}");
        }

        public static void FlushNow()
        {
            lock (flushSync)
            {
                FlushQueueToFile(logMessages, logFilePath);
                FlushQueueToFile(logErrorMessages, logErrorFilePath);
            }
        }

        public static void Shutdown()
        {
            try { flushTimer.Stop(); } catch { }
            FlushNow();
            try { flushTimer.Dispose(); } catch { }
        }

        private static void FlushQueueToFile(ConcurrentQueue<string> queue, string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;

            var batch = new List<string>(Math.Max(queue.Count, 1));
            while (queue.TryDequeue(out var msg))
            {
                if (msg is not null) batch.Add(msg);
            }
            if (batch.Count == 0) return;

            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            File.AppendAllLines(path, batch);
        }
    }
}
