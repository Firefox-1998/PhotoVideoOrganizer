using MetadataExtractor.Util;
using PhotoMoveYearMonthFolder.Resources;

namespace PhotoMoveYearMonthFolder.Services
{
    /// <summary>
    /// Enumerates and classifies files in a directory structure.
    /// Separates valid media files from other files.
    /// </summary>
    public sealed class FileEnumerator
    {
        #region Enums

        public enum MediaKind { Image, Video, Audio, Other }

        #endregion

        #region Constants

        private const int EnumerationProgressInterval = 100;

        #endregion

        #region Static Data

        private static readonly HashSet<string> ExcludedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".ini", ".db", ".com", ".exe", ".dll", ".txt"
        };

        private static readonly HashSet<string> KnownMediaExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            // Images
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp", ".heic", ".heif",
            ".psd", ".ico", ".pcx", ".tga", ".arw", ".crw", ".cr2", ".nef", ".orf", ".raf", ".rw2", ".cr3",
            // Videos
            ".mp4", ".mov", ".avi", ".mkv", ".wmv", ".flv", ".3gp", ".mpg", ".mpeg", ".webm",
            ".m4v", ".mts", ".m2ts", ".ts", ".3g2", ".asf", ".mxf", ".vob", ".ogv", ".mpe",
            ".divx", ".rm", ".rmvb", ".f4v", ".crm",
            // Audio
            ".mp3", ".wav", ".aac", ".flac", ".ogg", ".m4a", ".mka", ".weba", ".m4b", ".opus",
            ".oga", ".wma", ".aif", ".aiff", ".aifc", ".ape", ".alac"
        };

        private static readonly HashSet<string> FastPathImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".heic", ".heif", ".tiff", ".webp"
        };

        private static readonly HashSet<string> FastPathVideoExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp4", ".mov", ".avi", ".mkv", ".wmv", ".webm", ".m4v", ".mpg", ".mpeg"
        };

        private static readonly HashSet<string> FastPathAudioExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp3", ".wav", ".flac", ".aac", ".ogg", ".m4a", ".wma"
        };

        #endregion

        #region Public Methods

        /// <summary>
        /// Enumerates files in the specified directory and partitions them into valid media and other files.
        /// </summary>
        /// <param name="rootPath">The root directory to search.</param>
        /// <param name="rootOnly">If true, only searches the root directory; otherwise, searches recursively.</param>
        /// <returns>A tuple containing collections of valid media files and other files.</returns>
        public (IEnumerable<string> ValidFiles, IEnumerable<string> InvalidFiles) GetFiles(string rootPath, bool rootOnly)
        {
            EnumerationOptions options = new()
            {
                RecurseSubdirectories = !rootOnly,
                IgnoreInaccessible = true,
                AttributesToSkip = FileAttributes.System | FileAttributes.Hidden
            };

            IEnumerable<string> allFiles = EnumerateFilesSafe(rootPath, options);

            ILookup<bool, string> partitionedFiles = allFiles
                .Where(file => !ExcludedExtensions.Contains(Path.GetExtension(file)))
                .ToLookup(IsValidMediaBySniff);

            return (partitionedFiles[true], partitionedFiles[false]);
        }

        /// <summary>
        /// Asynchronously retrieves and materializes file lists.
        /// </summary>
        public Task<(string[] ValidFiles, string[] InvalidFiles)> GetFilesAsync(
            string rootPath, bool rootOnly, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                (IEnumerable<string> valid, IEnumerable<string> invalid) = GetFiles(rootPath, rootOnly);
                return (valid.ToArray(), invalid.ToArray());
            }, cancellationToken);
        }

        /// <summary>
        /// Asynchronously retrieves and materializes file lists, reporting enumeration progress.
        /// </summary>
        public Task<(string[] ValidFiles, string[] InvalidFiles)> GetFilesAsync(
            string rootPath,
            bool rootOnly,
            IProgressReporter progressReporter,
            CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                EnumerationOptions options = new()
                {
                    RecurseSubdirectories = !rootOnly,
                    IgnoreInaccessible = true,
                    AttributesToSkip = FileAttributes.System | FileAttributes.Hidden
                };

                IEnumerable<string> allFiles = EnumerateFilesSafe(rootPath, options);

                List<string> validFiles = [];
                List<string> invalidFiles = [];
                int totalProcessed = 0;

                foreach (string file in allFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (ExcludedExtensions.Contains(Path.GetExtension(file)))
                    {
                        continue;
                    }

                    if (IsValidMediaBySniff(file))
                    {
                        validFiles.Add(file);
                    }
                    else
                    {
                        invalidFiles.Add(file);
                    }

                    totalProcessed++;

                    if (totalProcessed == 1 || totalProcessed % EnumerationProgressInterval == 0)
                    {
                        progressReporter.ReportEnumerationProgress(
                            totalProcessed, validFiles.Count, invalidFiles.Count);
                    }
                }

                // Final progress report with exact counts
                progressReporter.ReportEnumerationProgress(
                    totalProcessed, validFiles.Count, invalidFiles.Count);

                return (validFiles.ToArray(), invalidFiles.ToArray());
            }, cancellationToken);
        }

        /// <summary>
        /// Determines if a file is a valid media file using extension lookup and file sniffing.
        /// </summary>
        public static bool IsValidMediaBySniff(string path)
        {
            string extension = Path.GetExtension(path);

            // Fast path: check known extensions first
            if (KnownMediaExtensions.Contains(extension))
            {
                return true;
            }

            // Slow path: perform file sniffing
            MediaKind kind = DetectMediaKind(path);
            return kind is MediaKind.Image or MediaKind.Video or MediaKind.Audio;
        }

        /// <summary>
        /// Detects the media kind using extension-based fast path first.
        /// </summary>
        public static MediaKind DetectMediaKindFast(string path)
        {
            string ext = Path.GetExtension(path);

            if (FastPathImageExtensions.Contains(ext))
            {
                return MediaKind.Image;
            }

            if (FastPathVideoExtensions.Contains(ext))
            {
                return MediaKind.Video;
            }

            if (FastPathAudioExtensions.Contains(ext))
            {
                return MediaKind.Audio;
            }

            return DetectMediaKind(path);
        }

        #endregion

        #region Private Methods

        private static IEnumerable<string> EnumerateFilesSafe(string rootPath, EnumerationOptions options)
        {
            try
            {
                DirectoryInfo root = new(rootPath);
                if ((root.Attributes & (FileAttributes.System | FileAttributes.Hidden)) != 0)
                {
                    return [];
                }

                return Directory.EnumerateFiles(rootPath, "*", options);
            }
            catch
            {
                return [];
            }
        }

        private static MediaKind DetectMediaKind(string path)
        {
            try
            {
                using FileStream stream = File.OpenRead(path);
                FileType ft = FileTypeDetector.DetectFileType(stream);

                return ft switch
                {
                    FileType.Jpeg or FileType.Png or FileType.Bmp or FileType.Gif or
                    FileType.WebP or FileType.Tiff or FileType.Psd or FileType.Ico or
                    FileType.Pcx or FileType.Netpbm or FileType.Eps or FileType.Tga or
                    FileType.Heif or FileType.Arw or FileType.Crw or FileType.Cr2 or
                    FileType.Nef or FileType.Orf or FileType.Raf or FileType.Rw2
                        => MediaKind.Image,

                    FileType.Mp3 or FileType.Wav
                        => MediaKind.Audio,

                    FileType.Mp4 or FileType.QuickTime
                        => DetectQuickTimeMediaKind(path),

                    FileType.Avi
                        => MediaKind.Video,

                    FileType.Crx
                        => DetectCrxMediaKind(path),

                    FileType.Riff
                        => DetectRiffMediaKind(path),

                    _
                        => DetectByExtensionFallback(path)
                };
            }
            catch
            {
                return MediaKind.Other;
            }
        }

        private static MediaKind DetectQuickTimeMediaKind(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();

            if (ext is ".m4a" or ".m4b")
            {
                return MediaKind.Audio;
            }

            return MediaKind.Video;
        }

        private static MediaKind DetectCrxMediaKind(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();

            return ext switch
            {
                ".crm" => MediaKind.Video,
                ".cr3" => MediaKind.Image,
                _ => MediaKind.Image
            };
        }

        private static MediaKind DetectRiffMediaKind(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();

            return ext switch
            {
                ".avi" => MediaKind.Video,
                ".wav" => MediaKind.Audio,
                ".webp" => MediaKind.Image,
                _ => MediaKind.Other
            };
        }

        private static MediaKind DetectByExtensionFallback(string path)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();

            // Video containers not in FileType enum
            if (ext is ".mkv" or ".webm" or ".m4v" or ".mts" or ".m2ts" or ".ts" or
                ".3gp" or ".3g2" or ".wmv" or ".asf" or ".mxf" or ".vob" or
                ".ogv" or ".mpg" or ".mpeg" or ".mpe" or ".divx" or ".rm" or
                ".rmvb" or ".f4v")
            {
                return MediaKind.Video;
            }

            // Audio containers not in FileType enum
            if (ext is ".mka" or ".weba" or ".m4a" or ".m4b" or ".aac" or ".flac" or
                ".ogg" or ".opus" or ".oga" or ".wma" or ".aif" or ".aiff" or
                ".aifc" or ".ape" or ".alac")
            {
                return MediaKind.Audio;
            }

            return MediaKind.Other;
        }

        #endregion
    }
}