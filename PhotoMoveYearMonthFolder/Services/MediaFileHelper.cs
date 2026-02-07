using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace PhotoMoveYearMonthFolder.Services
{
    /// <summary>
    /// Helper methods for media file operations including
    /// EXIF reading, file copying, and filename generation.
    /// </summary>
    public static class MediaFileHelper
    {
        #region Constants

        // Buffer size for file copy operations (1MB for better throughput)
        private const int CopyBufferSize = 1024 * 1024;

        #endregion

        #region EXIF Methods

        /// <summary>
        /// Reads the EXIF UniqueImageID from an image file.
        /// Returns empty string if not available or on error.
        /// </summary>
        public static string ReadExifUniqueImageID(string filePath)
        {
            try
            {
                IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(filePath);
                ExifIfd0Directory? exifIfd0Directory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
                return exifIfd0Directory?.GetString(ExifIfd0Directory.TagImageUniqueId) ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Reads the date from EXIF metadata or falls back to file system date.
        /// Returns date in "yyyyMMdd" format.
        /// </summary>
        public static string ReadExifDataOrFileSystemDate(string filePath)
        {
            try
            {
                IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(filePath);
                ExifSubIfdDirectory? subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

                if (subIfdDirectory != null &&
                    subIfdDirectory.TryGetDateTime(ExifSubIfdDirectory.TagDateTimeOriginal, out DateTime dateTime))
                {
                    return dateTime.ToString("yyyyMMdd");
                }
            }
            catch
            {
                // On error reading EXIF, proceed with file system date
            }

            return File.GetLastWriteTime(filePath).ToString("yyyyMMdd");
        }

        #endregion

        #region File Type Detection

        /// <summary>
        /// Fast check if a file is a common image format by extension.
        /// </summary>
        public static bool IsImageFileFast(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();

            return ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or
                   ".tiff" or ".webp" or ".heic" or ".heif";
        }

        #endregion

        #region File Operations

        /// <summary>
        /// Generates a unique file name by appending a counter if the file exists.
        /// Example: "photo.jpg" -> "photo_1.jpg" -> "photo_2.jpg"
        /// </summary>
        public static string GenerateUniqueFileName(string originalPath)
        {
            string? directory = Path.GetDirectoryName(originalPath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalPath);
            string extension = Path.GetExtension(originalPath);

            int counter = 1;
            string newFullPath;

            do
            {
                string newFileName = $"{fileNameWithoutExtension}_{counter}{extension}";
                newFullPath = Path.Combine(directory ?? string.Empty, newFileName);
                counter++;
            }
            while (File.Exists(newFullPath));

            return newFullPath;
        }

        /// <summary>
        /// Copies a file asynchronously with optimized buffer size.
        /// If destination exists, generates a unique name.
        /// Returns the actual destination path used.
        /// </summary>
        public static async Task<string> CopyFileAsync(string sourceFile, string destinationFile)
        {
            string finalDestination = destinationFile;

            if (File.Exists(destinationFile))
            {
                finalDestination = GenerateUniqueFileName(destinationFile);
            }

            await using FileStream sourceStream = new(
                sourceFile,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                CopyBufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            await using FileStream destinationStream = new(
                finalDestination,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                CopyBufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            await sourceStream.CopyToAsync(destinationStream);

            return finalDestination;
        }

        #endregion
    }
}