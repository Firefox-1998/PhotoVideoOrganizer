using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace PhotoMoveYearMonthFolder.Services
{
    /// <summary>
    /// Detects duplicate files using multiple strategies:
    /// - Name and size tracking
    /// - Partial content comparison
    /// - Full SHA-256 hash comparison
    /// - EXIF UniqueImageID comparison
    /// Includes hash caching for performance optimization.
    /// </summary>
    public sealed class DuplicateDetector
    {
        #region Constants

        // Buffer size for hash computation (1MB)
        private const int HashBufferSize = 1024 * 1024;

        // Sample size for partial file comparison (8KB)
        private const int PartialComparisonSampleSize = 8192;

        #endregion

        #region Fields

        private ConcurrentDictionary<(string Name, long Size), byte> _processedFileKeys;
        private readonly ConcurrentDictionary<string, string> _hashCache;

        #endregion

        #region Constructor

        public DuplicateDetector()
        {
            _processedFileKeys = new ConcurrentDictionary<(string Name, long Size), byte>();
            _hashCache = new ConcurrentDictionary<string, string>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets the tracking state for a new processing batch.
        /// </summary>
        public void Reset()
        {
            _processedFileKeys = new ConcurrentDictionary<(string Name, long Size), byte>();
            _hashCache.Clear();
        }

        /// <summary>
        /// Attempts to register a file for processing.
        /// Returns false if a file with the same name and size was already processed.
        /// </summary>
        public bool TryRegisterFile(FileInfo fileInfo)
        {
            (string Name, long Length) fileKey = (fileInfo.Name, fileInfo.Length);
            return _processedFileKeys.TryAdd(fileKey, 0);
        }

        /// <summary>
        /// Checks if two files are identical using a multi-stage comparison:
        /// 1. Size comparison (fast fail)
        /// 2. Partial content comparison (first and last 8KB)
        /// 3. Full SHA-256 hash comparison
        /// </summary>
        public bool AreFilesIdentical(string file1, string file2)
        {
            FileInfo fi1 = new(file1);
            FileInfo fi2 = new(file2);

            // Stage 1: Fast size comparison
            if (fi1.Length != fi2.Length)
            {
                return false;
            }

            // Stage 2: Partial content comparison
            if (!ComparePartialContent(file1, file2, fi1.Length))
            {
                return false;
            }

            // Stage 3: Full hash comparison
            string hash1 = GetOrComputeHash(file1);
            string hash2 = GetOrComputeHash(file2);

            return string.Equals(hash1, hash2, StringComparison.Ordinal);
        }

        /// <summary>
        /// Checks if two image files have the same EXIF UniqueImageID.
        /// Only applicable for image files with EXIF metadata.
        /// </summary>
        public static bool HaveSameExifId(string sourceFile, string destinationFile)
        {
            if (!MediaFileHelper.IsImageFileFast(sourceFile))
            {
                return false;
            }

            string srcId = MediaFileHelper.ReadExifUniqueImageID(sourceFile);
            if (string.IsNullOrEmpty(srcId))
            {
                return false;
            }

            string dstId = MediaFileHelper.ReadExifUniqueImageID(destinationFile);
            return string.Equals(srcId, dstId, StringComparison.Ordinal);
        }

        /// <summary>
        /// Computes the SHA-256 hash of a file, using cache when available.
        /// </summary>
        public string GetOrComputeHash(string filePath)
        {
            return _hashCache.GetOrAdd(filePath, ComputeHash);
        }

        #endregion

        #region Private Methods

        private static bool ComparePartialContent(string file1, string file2, long fileLength)
        {
            byte[] buffer1 = new byte[PartialComparisonSampleSize];
            byte[] buffer2 = new byte[PartialComparisonSampleSize];

            using FileStream fs1 = new(file1, FileMode.Open, FileAccess.Read, FileShare.Read, PartialComparisonSampleSize, FileOptions.SequentialScan);
            using FileStream fs2 = new(file2, FileMode.Open, FileAccess.Read, FileShare.Read, PartialComparisonSampleSize, FileOptions.SequentialScan);

            // Compare beginning of files
            int read1 = fs1.Read(buffer1, 0, PartialComparisonSampleSize);
            int read2 = fs2.Read(buffer2, 0, PartialComparisonSampleSize);

            if (read1 != read2 || !buffer1.AsSpan(0, read1).SequenceEqual(buffer2.AsSpan(0, read2)))
            {
                return false;
            }

            // If file is small enough, we already compared everything
            if (fileLength <= PartialComparisonSampleSize)
            {
                return true;
            }

            // Compare end of files
            long tailOffset = Math.Max(0, fileLength - PartialComparisonSampleSize);
            fs1.Seek(tailOffset, SeekOrigin.Begin);
            fs2.Seek(tailOffset, SeekOrigin.Begin);

            read1 = fs1.Read(buffer1, 0, PartialComparisonSampleSize);
            read2 = fs2.Read(buffer2, 0, PartialComparisonSampleSize);

            return read1 == read2 && buffer1.AsSpan(0, read1).SequenceEqual(buffer2.AsSpan(0, read2));
        }

        private static string ComputeHash(string filePath)
        {
            using SHA256 sha256 = SHA256.Create();
            using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, HashBufferSize, FileOptions.SequentialScan);
            byte[] hash = sha256.ComputeHash(stream);
            return Convert.ToHexStringLower(hash);
        }

        #endregion
    }
}