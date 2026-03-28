using PhotoMoveYearMonthFolder.Resources;

namespace PhotoMoveYearMonthFolder.Services
{
    /// <summary>
    /// Orchestrates the file processing workflow including:
    /// - File enumeration and classification
    /// - Duplicate detection
    /// - Date extraction and folder organization
    /// - Progress reporting
    /// </summary>
    public sealed class FileProcessingService
    {
        #region Constants

        private const string OtherFilesFolder = "OtherFilesExt";
        private const int UiUpdateInterval = 10;

        #endregion

        #region Fields

        private readonly DateExtractor _dateExtractor;
        private readonly DuplicateDetector _duplicateDetector;
        private readonly FileEnumerator _fileEnumerator;
        private readonly IProgressReporter _progressReporter;

        private int _processedMediaCount;
        private int _processedOtherCount;

        #endregion

        #region Constructor

        public FileProcessingService(IProgressReporter progressReporter)
        {
            _dateExtractor = new DateExtractor();
            _duplicateDetector = new DuplicateDetector();
            _fileEnumerator = new FileEnumerator();
            _progressReporter = progressReporter;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Processes all files from source to destination directory.
        /// </summary>
        public async Task ProcessAllFilesAsync(
            string sourceDir,
            string destDir,
            bool rootOnly,
            int maxThreads,
            CancellationToken cancellationToken)
        {
            ResetState();

            // Enumerate and classify files with progress reporting
            (string[] validFiles, string[] invalidFiles) = await _fileEnumerator
                .GetFilesAsync(sourceDir, rootOnly, _progressReporter, cancellationToken);

            // Process valid media files
            await ProcessMediaFilesAsync(validFiles, destDir, maxThreads, cancellationToken);

            // Reset duplicate detector for second batch
            _duplicateDetector.Reset();

            // Process other files
            await ProcessOtherFilesAsync(invalidFiles, destDir, maxThreads, cancellationToken);
        }

        /// <summary>
        /// Processes only valid media files.
        /// </summary>
        public async Task ProcessMediaFilesAsync(
            string[] files,
            string destDir,
            int maxThreads,
            CancellationToken cancellationToken)
        {
            _progressReporter.SetMediaProgressMarquee();
            Logger.Log(Logging.Btn_Start_Click_STARTVALIDMEDIA);

            ParallelOptions options = new()
            {
                MaxDegreeOfParallelism = maxThreads,
                CancellationToken = cancellationToken
            };

            await Parallel.ForEachAsync(files, options, async (file, ct) =>
            {
                await ProcessSingleMediaFileAsync(file, destDir, ct);
            });

            Logger.Log(Logging.Btn_Start_Click_ENDVALIDMEDIA);
            _progressReporter.CompleteMediaProgress();
        }

        /// <summary>
        /// Processes non-media files.
        /// </summary>
        public async Task ProcessOtherFilesAsync(
            string[] files,
            string destDir,
            int maxThreads,
            CancellationToken cancellationToken)
        {
            _progressReporter.SetOtherProgressMarquee();
            Logger.Log(Logging.Btn_Start_Click_STARTNOTVALIDMEDIA);

            ParallelOptions options = new()
            {
                MaxDegreeOfParallelism = maxThreads,
                CancellationToken = cancellationToken
            };

            await Parallel.ForEachAsync(files, options, async (file, ct) =>
            {
                await ProcessSingleOtherFileAsync(file, destDir, ct);
            });

            Logger.Log(Logging.Btn_Start_Click_ENDNOTVALIDMEDIA);
            _progressReporter.CompleteOtherProgress();
        }

        #endregion

        #region Private Methods - Processing

        private void ResetState()
        {
            _duplicateDetector.Reset();
            _processedMediaCount = 0;
            _processedOtherCount = 0;
            _progressReporter.ResetProgress();
        }

        private async Task ProcessSingleMediaFileAsync(
            string sourceFile,
            string destDir,
            CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                FileInfo fileInfo = new(sourceFile);

                if (!_duplicateDetector.TryRegisterFile(fileInfo))
                {
                    Logger.Log(string.Format(Logging.ProcessFileAsync_SkippedNameAndSizeAlreadySeen, sourceFile));
                    return;
                }

                string destinationPath = BuildMediaDestinationPath(sourceFile, fileInfo, destDir);

                await CopyIfNotDuplicateAsync(
                    sourceFile,
                    destinationPath,
                    isMediaFile: true,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(string.Format(Logging.ProcessFileAsync_ErrorDuringProcessingOf, sourceFile, ex.Message));
            }
            finally
            {
                ReportMediaProgress(sourceFile, cancellationToken);
            }
        }

        private async Task ProcessSingleOtherFileAsync(
            string sourceFile,
            string destDir,
            CancellationToken cancellationToken)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                FileInfo fileInfo = new(sourceFile);

                if (!_duplicateDetector.TryRegisterFile(fileInfo))
                {
                    Logger.Log(string.Format(Logging.ProcessFileAsync_SkippedNameAndSizeAlreadySeen, sourceFile));
                    return;
                }

                string otherFilesDir = Path.Combine(destDir, OtherFilesFolder);
                Directory.CreateDirectory(otherFilesDir);

                string destinationPath = Path.Combine(otherFilesDir, fileInfo.Name);

                await CopyIfNotDuplicateAsync(
                    sourceFile,
                    destinationPath,
                    isMediaFile: false,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(string.Format(Logging.ProcessFileAsync_ErrorDuringProcessingOf, sourceFile, ex.Message));
            }
            finally
            {
                ReportOtherProgress(sourceFile, cancellationToken);
            }
        }

        private string BuildMediaDestinationPath(string sourceFile, FileInfo fileInfo, string destDir)
        {
            string fileName = Path.GetFileNameWithoutExtension(sourceFile);
            (string year, string month) = _dateExtractor.ExtractYearMonth(fileName, sourceFile);

            string yearFolder = Path.Combine(destDir, year);
            Directory.CreateDirectory(yearFolder);

            string monthFolder = Path.Combine(yearFolder, month);
            Directory.CreateDirectory(monthFolder);

            return Path.Combine(monthFolder, fileInfo.Name);
        }

        private async Task CopyIfNotDuplicateAsync(
            string sourceFile,
            string destinationPath,
            bool isMediaFile,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string logFormat = isMediaFile
                ? Logging.ProcessFileAsyncValidExt_Copied
                : Logging.ProcessFileAsyncNotValidExt_Copied;

            if (File.Exists(destinationPath))
            {
                // Check if files are identical
                if (_duplicateDetector.AreFilesIdentical(sourceFile, destinationPath))
                {
                    Logger.Log(string.Format(
                        Logging.ProcessFileAsync_SkippedDuplicateConfirmedByHash,
                        sourceFile,
                        destinationPath));
                    return;
                }

                // Additional EXIF check for media files
                if (isMediaFile && DuplicateDetector.HaveSameExifId(sourceFile, destinationPath))
                {
                    Logger.Log(string.Format(
                        Logging.ProcessFileAsync_SkippedSameEXIFUniqueImageID,
                        sourceFile,
                        destinationPath));
                    return;
                }

                // File exists but is different - copy with new name
                string newDestPath = MediaFileHelper.GenerateUniqueFileName(destinationPath);
                string writtenPath = await MediaFileHelper.CopyFileAsync(sourceFile, newDestPath);
                Logger.Log(string.Format(logFormat, sourceFile, writtenPath));
            }
            else
            {
                string writtenPath = await MediaFileHelper.CopyFileAsync(sourceFile, destinationPath);
                Logger.Log(string.Format(logFormat, sourceFile, writtenPath));
            }
        }

        #endregion

        #region Private Methods - Progress

        private void ReportMediaProgress(string file, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            int count = Interlocked.Increment(ref _processedMediaCount);

            if (ShouldUpdateProgress(count))
            {
                _progressReporter.ReportMediaFileProgress(count, file);
            }
        }

        private void ReportOtherProgress(string file, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            int count = Interlocked.Increment(ref _processedOtherCount);

            if (ShouldUpdateProgress(count))
            {
                _progressReporter.ReportOtherFileProgress(count, file);
            }
        }

        private static bool ShouldUpdateProgress(int count)
        {
            return count == 1 || count % UiUpdateInterval == 0;
        }

        #endregion
    }
}