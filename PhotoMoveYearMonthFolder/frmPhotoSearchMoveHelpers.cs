using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.QuickTime;
using MetadataExtractor.Util;
using System.Security.Cryptography;
using System.Text;

namespace PhotoMoveYearMonthFolder
{
    public static class FrmPhotoSearchMoveHelpers
    {
        private static readonly HashSet<string> KnownMediaExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            // Immagini
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp", ".heic", ".heif",
            ".psd", ".ico", ".pcx", ".tga", ".arw", ".crw", ".cr2", ".nef", ".orf", ".raf", ".rw2", ".cr3",
            // Video
            ".mp4", ".mov", ".avi", ".mkv", ".wmv", ".flv", ".3gp", ".mpg", ".mpeg", ".webm",
            ".m4v", ".mts", ".m2ts", ".ts", ".3g2", ".asf", ".mxf", ".vob", ".ogv", ".mpe",
            ".divx", ".rm", ".rmvb", ".f4v", ".crm",
            // Audio
            ".mp3", ".wav", ".aac", ".flac", ".ogg", ".m4a", ".mka", ".weba", ".m4b", ".opus",
            ".oga", ".wma", ".aif", ".aiff", ".aifc", ".ape", ".alac"
        };

        public enum MediaKind { Image, Video, Audio, Other }

        // Sniff tipo file tramite MetadataExtractor.Util.FileTypeDetector
        public static MediaKind DetectMediaKind(string path)
        {
            try
            {
                using FileStream stream = File.OpenRead(path);
                FileType ft = FileTypeDetector.DetectFileType(stream);

                // 1) Mappatura diretta dei tipi supportati dall'enum
                switch (ft)
                {
                    case FileType.Jpeg:
                    case FileType.Png:
                    case FileType.Bmp:
                    case FileType.Gif:
                    case FileType.WebP:
                    case FileType.Tiff:
                    case FileType.Psd:
                    case FileType.Ico:
                    case FileType.Pcx:
                    case FileType.Netpbm:
                    case FileType.Eps:
                    case FileType.Tga:
                    case FileType.Heif:
                    case FileType.Arw:
                    case FileType.Crw:
                    case FileType.Cr2:
                    case FileType.Nef:
                    case FileType.Orf:
                    case FileType.Raf:
                    case FileType.Rw2:
                        return MediaKind.Image;

                    case FileType.Mp3:
                    case FileType.Wav:
                        return MediaKind.Audio;

                    case FileType.Mp4:
                    case FileType.QuickTime:
                        {
                            // Heuristica estensione: alcuni contenitori ISO-BMFF audio-only
                            string ext = Path.GetExtension(path).ToLowerInvariant();
                            if (ext is ".m4a" or ".m4b") return MediaKind.Audio; // veloce e pragmatico

                            // Ispezione del container: tracce audio vs video
                            if (IsQuickTimeLikeAudioOnly(path)) return MediaKind.Audio;
                            return MediaKind.Video;
                        }

                    case FileType.Avi:
                        return MediaKind.Video;

                    // 2) Ambigui o generici: applica override per estensioni NON presenti nell'enum
                    case FileType.Crx:
                        {
                            // CRX è condiviso tra CR3 (image) e CRM (video)
                            string ext = Path.GetExtension(path).ToLowerInvariant();
                            if (ext == ".crm") return MediaKind.Video;
                            if (ext == ".cr3") return MediaKind.Image;
                            return MediaKind.Image; // default più probabile
                        }

                    case FileType.Riff:
                        {
                            // RIFF generico (se non già classificato come Avi/Wav/WebP)
                            string ext = Path.GetExtension(path).ToLowerInvariant();
                            if (ext == ".avi") return MediaKind.Video;
                            if (ext == ".wav") return MediaKind.Audio;
                            if (ext == ".webp") return MediaKind.Image;
                            return MediaKind.Other;
                        }

                    case FileType.Unknown:
                    default:
                        {
                            string ext = Path.GetExtension(path).ToLowerInvariant();

                            // 3) Override per formati NON compresi nell'enum
                            // Video container non presenti nell'enum
                            if (ext is ".mkv" or ".webm" or ".m4v" or ".mts" or ".m2ts" or ".ts"
                                or ".3gp" or ".3g2" or ".wmv" or ".asf" or ".mxf" or ".vob"
                                or ".ogv" or ".mpg" or ".mpeg" or ".mpe" or ".divx" or ".rm"
                                or ".rmvb" or ".f4v")
                                return MediaKind.Video;

                            // Audio container non presenti nell'enum
                            if (ext is ".mka" or ".weba" or ".m4a" or ".m4b" or ".aac" or ".flac"
                                or ".ogg" or ".opus" or ".oga" or ".wma" or ".aif" or ".aiff"
                                or ".aifc" or ".ape" or ".alac")
                                return MediaKind.Audio;

                            return MediaKind.Other;
                        }
                }
            }
            catch
            {
                return MediaKind.Other;
            }
        }

        // Rileva se un contenitore MP4/QuickTime è audio-only (nessuna traccia video)
        // 1) Brand ftyp: M4A/M4B => audio
        // 2) Tracce tkhd: se esiste width/height > 0 => video
        private static bool IsQuickTimeLikeAudioOnly(string path)
        {
            try
            {
                using FileStream fs = File.OpenRead(path);
                IEnumerable<MetadataExtractor.Directory> directories = QuickTimeMetadataReader.ReadMetadata(fs);

                // 1) Controllo brand dal box ftyp
                QuickTimeFileTypeDirectory? ftyp = directories.OfType<QuickTimeFileTypeDirectory>().FirstOrDefault();
                if (ftyp != null)
                {
                    string? major = ftyp.GetString(1);            // Tag 1 = MajorBrand
                    string[] compat = ftyp.GetStringArray(3) ?? [];    // Tag 3 = CompatibleBrands

                    static string NormalizeBrand(string? s) => (s ?? "").Replace(" ", "").ToUpperInvariant();
                    string majorN = NormalizeBrand(major);
                    string[] compatN = [.. compat.Select(NormalizeBrand)];

                    if (majorN is "M4A" or "M4B" || compatN.Any(b => b is "M4A" or "M4B"))
                        return true;   // audio-only
                    if (majorN is "M4V" || compatN.Any(b => b == "M4V"))
                        return false;  // video
                }

                // 2) Verifica delle tracce: se c'è almeno una traccia con dimensioni > 0 è video
                bool hasVideoTrack = directories
                    .OfType<QuickTimeTrackHeaderDirectory>()
                    .Any(t =>
                    {
                        // Tag 10 = Width, Tag 11 = Height (32-bit fixed point)
                        object? wObj = t.GetObject(10);
                        object? hObj = t.GetObject(11);

                        static double ToDouble(object? o) => o switch
                        {
                            sbyte v => v,
                            byte v => v,
                            short v => v,
                            ushort v => v,
                            int v => v,
                            uint v => v,
                            long v => v,
                            ulong v => v,
                            float v => v,
                            double v => v,
                            decimal v => (double)v,
                            _ => 0d
                        };

                        double w = ToDouble(wObj);
                        double h = ToDouble(hObj);
                        return w > 0 && h > 0;
                    });

                return !hasVideoTrack; // nessuna traccia con dimensioni => audio-only
            }
            catch
            {
                return false; // in dubbio, non affermare audio-only
            }
        }

        public static bool IsValidMediaBySniff(string path)
        {
            // Percorso veloce: controlla prima le estensioni note.
            string extension = Path.GetExtension(path);
            if (KnownMediaExtensions.Contains(extension))
            {
                return true;
            }

            // Percorso lento: se l'estensione non è nota o assente, esegui lo sniffing.
            return DetectMediaKind(path) is MediaKind.Image or MediaKind.Video or MediaKind.Audio;
        }

        public static string ComputeHash(string filePath)
        {
            using SHA256 sha256 = SHA256.Create();
            using FileStream stream = File.OpenRead(filePath);
            byte[] hash = sha256.ComputeHash(stream);
            return Convert.ToHexStringLower(hash);
        }

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

        public static bool IsImageFileFast(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".tiff" or ".webp" or ".heic" or ".heif" => true,
                _ => false,
            };
        }

        public static string GenerateNewFileName(string originalPath)
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
            } while (File.Exists(newFullPath));
            return newFullPath;
        }

        public static async Task<string> CopyFileWithUniqueNameAsync(string sourceFile, string destinationFile)
        {
            string finalDestination = destinationFile;
            if (File.Exists(destinationFile))
            {
                finalDestination = GenerateNewFileName(destinationFile);
            }

            const int bufferSize = 81920; // Dimensione buffer predefinita per CopyToAsync

            using (FileStream sourceStream = new(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.Asynchronous))
            using (FileStream destinationStream = new(finalDestination, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize, FileOptions.Asynchronous))
            {
                await sourceStream.CopyToAsync(destinationStream);
            }

            return finalDestination;
        }

        public static string ReadExifDataOrFileSystemDate(string file)
        {
            try
            {
                IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(file);
                ExifSubIfdDirectory? subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();

                if (subIfdDirectory != null)
                {
                    if (subIfdDirectory.TryGetDateTime(ExifSubIfdDirectory.TagDateTimeOriginal, out DateTime dateTime))
                    {
                        return dateTime.ToString("yyyyMMdd");
                    }
                }
            }
            catch
            {
                // In caso di errore nella lettura EXIF, procedi con la data del file system
            }

            return File.GetLastWriteTime(file).ToString("yyyyMMdd");
        }
    }
}