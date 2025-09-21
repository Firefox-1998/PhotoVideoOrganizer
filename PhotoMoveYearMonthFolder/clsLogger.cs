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

        public static void SetLogFilePath(string path)
        {
            _logFilePath = path;
            Initialize(ref _logQueue, ref _logTask, _logFilePath);
        }

        public static void SetErrorFilePath(string path)
        {
            _errorFilePath = path;
            Initialize(ref _errorQueue, ref _errorTask, _errorFilePath);
        }

        private static void Initialize(ref BlockingCollection<string>? queue, ref Task? task, string filePath)
        {
            // Se l'attività esiste già, completa la coda e attendi la sua terminazione.
            queue?.CompleteAdding();
            task?.Wait();

            // Crea una nuova coda come variabile locale.
            BlockingCollection<string> newQueue = new(new ConcurrentQueue<string>());
            
            // Assegna la nuova coda al parametro ref per aggiornare il campo statico.
            queue = newQueue;

            // Avvia l'attività di elaborazione catturando la variabile locale, non il parametro ref.
            task = Task.Run(() => ProcessQueue(newQueue, filePath));
            task.ContinueWith(t => {
                // Gestisci eventuali eccezioni impreviste dal thread di log.
                System.Diagnostics.Debug.WriteLine($"FATAL: Logger task failed. Exception: {t.Exception}");
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public static void Log(string message)
        {
            // Non bloccare se la coda è piena (improbabile con una coda non limitata).
            _logQueue?.TryAdd($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {message}");
        }

        public static void LogError(string message)
        {
            _errorQueue?.TryAdd($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {message}");
        }

        private static void ProcessQueue(BlockingCollection<string> queue, string filePath)
        {
            try
            {
                // Assicura che la directory esista.
                string? dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                using StreamWriter streamWriter = new(filePath, append: false, Encoding.UTF8);
                
                // GetConsumingEnumerable si blocca in attesa di elementi e termina
                // quando la coda è vuota e CompleteAdding() è stato chiamato.
                foreach (string message in queue.GetConsumingEnumerable())
                {
                    streamWriter.WriteLine(message);
                }
                // Svuota eventuali buffer rimanenti prima di chiudere lo stream.
                streamWriter.Flush();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FATAL: Logger failed for {filePath}. Reason: {ex.Message}");
            }
        }

        public static void FlushNow()
        {
            // Questo metodo non è più necessario. Il thread di background gestisce lo svuotamento.
            // Lo lasciamo vuoto per mantenere la compatibilità con le chiamate esistenti.
        }

        public static void Shutdown()
        {
            // Segnala che non verranno più aggiunti elementi alle code.
            // Questo farà terminare il ciclo foreach in ProcessQueue.
            _logQueue?.CompleteAdding();
            _errorQueue?.CompleteAdding();

            // Attendi che entrambe le attività di log completino la scrittura degli elementi rimanenti.
            Task.WaitAll(_logTask ?? Task.CompletedTask, _errorTask ?? Task.CompletedTask);

            // Rilascia le risorse delle code.
            _logQueue?.Dispose();
            _errorQueue?.Dispose();
        }
    }
}
