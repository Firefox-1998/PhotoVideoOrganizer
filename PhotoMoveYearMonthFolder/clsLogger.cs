using System.Collections.Concurrent;
using System.Text;

namespace PhotoMoveYearMonthFolder
{
    public static class Logger
    {
        private static BlockingCollection<string>? _logQueue;
        private static BlockingCollection<string>? _errorQueue;
        private static Task? _logTask;
        private static Task? _errorTask;
        private static string? _logFilePath;
        private static string? _errorFilePath;
        private static volatile bool _isShuttingDown;
        private static readonly Lock _initLock = new();

        public static void SetLogFilePath(string path)
        {
            lock (_initLock)
            {
                if (_isShuttingDown) return;
                _logFilePath = path;
                Initialize(ref _logQueue, ref _logTask, _logFilePath);
            }
        }

        public static void SetErrorFilePath(string path)
        {
            lock (_initLock)
            {
                if (_isShuttingDown) return;
                _errorFilePath = path;
                Initialize(ref _errorQueue, ref _errorTask, _errorFilePath);
            }
        }

        private static void Initialize(ref BlockingCollection<string>? queue, ref Task? task, string filePath)
        {
            // If the task already exists, complete the queue and wait for its termination.
            try
            {
                queue?.CompleteAdding();
            }
            catch (ObjectDisposedException) { }

            try
            {
                task?.Wait();
            }
            catch (AggregateException) { }
            catch (ObjectDisposedException) { }

            // Create a new queue as a local variable.
            BlockingCollection<string> newQueue = new(new ConcurrentQueue<string>());

            // Assign the new queue to the ref parameter to update the static field.
            queue = newQueue;

            // Start the processing task capturing the local variable, not the ref parameter.
            task = Task.Run(() => ProcessQueue(newQueue, filePath));
            task.ContinueWith(t => {
                // Handle any unexpected exceptions from the log thread.
                System.Diagnostics.Debug.WriteLine($"FATAL: Logger task failed. Exception: {t.Exception}");
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public static void Log(string message)
        {
            if (_isShuttingDown) return;

            try
            {
                // Do not block if the queue is full (unlikely with an unbounded queue).
                _logQueue?.TryAdd($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {message}");
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }

        public static void LogError(string message)
        {
            if (_isShuttingDown) return;

            try
            {
                _errorQueue?.TryAdd($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {message}");
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }

        private static void ProcessQueue(BlockingCollection<string> queue, string filePath)
        {
            try
            {
                // Ensure the directory exists.
                string? dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                using StreamWriter streamWriter = new(filePath, append: false, Encoding.UTF8);

                // GetConsumingEnumerable blocks waiting for items and terminates
                // when the queue is empty and CompleteAdding() has been called.
                foreach (string message in queue.GetConsumingEnumerable())
                {
                    streamWriter.WriteLine(message);
                }
                // Flush any remaining buffers before closing the stream.
                streamWriter.Flush();
            }
            catch (ObjectDisposedException)
            {
                // The queue was disposed during shutdown, this is normal.
            }
            catch (InvalidOperationException)
            {
                // CompleteAdding was called, this is normal during shutdown.
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FATAL: Logger failed for {filePath}. Reason: {ex.Message}");
            }
        }

        public static void FlushNow()
        {
            // This method is no longer needed. The background thread handles flushing.
            // We leave it empty to maintain compatibility with existing calls.
        }

        public static void Shutdown()
        {
            lock (_initLock)
            {
                if (_isShuttingDown) return;
                _isShuttingDown = true;
            }

            // Signal that no more items will be added to the queues.
            // This will terminate the foreach loop in ProcessQueue.
            try { _logQueue?.CompleteAdding(); } catch (ObjectDisposedException) { }
            try { _errorQueue?.CompleteAdding(); } catch (ObjectDisposedException) { }

            // Wait for both log tasks to complete writing any remaining items.
            try
            {
                Task[] tasks = [_logTask ?? Task.CompletedTask, _errorTask ?? Task.CompletedTask];
                Task.WaitAll(tasks, TimeSpan.FromSeconds(5)); // Timeout to avoid indefinite blocking
            }
            catch (AggregateException) { }
            catch (ObjectDisposedException) { }

            // Release queue resources.
            try { _logQueue?.Dispose(); } catch (ObjectDisposedException) { }
            try { _errorQueue?.Dispose(); } catch (ObjectDisposedException) { }

            _logQueue = null;
            _errorQueue = null;
            _logTask = null;
            _errorTask = null;
        }

        public static void Reset()
        {
            lock (_initLock)
            {
                _isShuttingDown = false;
            }
        }
    }
}