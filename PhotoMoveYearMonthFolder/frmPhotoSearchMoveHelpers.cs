using System.Security.Cryptography;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.QuickTime;
using MetadataExtractor.Util;

internal static partial class FrmPhotoSearchMoveHelpers
{
    public enum MediaKind { Image, Video, Audio, Other }

    // Copia asincrona: FileStream + CopyToAsync, crea la cartella di destinazione
    public static async Task CopyFileAsync(string sourceFile, string destinationFile)
    {
        var dir = Path.GetDirectoryName(destinationFile);
        if (!string.IsNullOrEmpty(dir)) System.IO.Directory.CreateDirectory(dir);

        await using FileStream src = new(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, useAsync: true);
        await using FileStream dst = new(destinationFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1024 * 1024, useAsync: true);
        await src.CopyToAsync(dst);
    }

    // Copia atomica con retry su nome unico (evita race tra thread)
    public static async Task<string> CopyFileWithUniqueNameAsync(string sourceFile, string destinationFile)
    {
        var dir = Path.GetDirectoryName(destinationFile);
        if (!string.IsNullOrEmpty(dir)) System.IO.Directory.CreateDirectory(dir);

        string dest = destinationFile;
        while (true)
        {
            try
            {
                await using FileStream src = new(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, useAsync: true);
                await using FileStream dst = new(dest, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1024 * 1024, useAsync: true);
                await src.CopyToAsync(dst);
                return dest;
            }
            catch (IOException)
            {
                dest = GenerateNewFileName(destinationFile);
            }
        }
    }

    // Hash normalizzato: esadecimale lowercase, senza trattini
    public static string ComputeHash(string file)
    {
        using var sha256 = SHA256.Create();
        using FileStream stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
        var hash = sha256.ComputeHash(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

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
                    var ext = Path.GetExtension(path).ToLowerInvariant();
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
                    var ext = Path.GetExtension(path).ToLowerInvariant();
                    if (ext == ".crm") return MediaKind.Video;
                    if (ext == ".cr3") return MediaKind.Image;
                    return MediaKind.Image; // default più probabile
                }

                case FileType.Riff:
                {
                    // RIFF generico (se non già classificato come Avi/Wav/WebP)
                    var ext = Path.GetExtension(path).ToLowerInvariant();
                    if (ext == ".avi") return MediaKind.Video;
                    if (ext == ".wav") return MediaKind.Audio;
                    if (ext == ".webp") return MediaKind.Image;
                    return MediaKind.Other;
                }

                case FileType.Unknown:
                default:
                {
                    var ext = Path.GetExtension(path).ToLowerInvariant();

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
            using var fs = File.OpenRead(path);
            var directories = QuickTimeMetadataReader.ReadMetadata(fs);

            // 1) Controllo brand dal box ftyp
            var ftyp = directories.OfType<QuickTimeFileTypeDirectory>().FirstOrDefault();
            if (ftyp != null)
            {
                string? major = ftyp.GetString(1);            // Tag 1 = MajorBrand
                var compat = ftyp.GetStringArray(3) ?? [];    // Tag 3 = CompatibleBrands

                static string NormalizeBrand(string? s) => (s ?? "").Replace(" ", "").ToUpperInvariant();
                var majorN = NormalizeBrand(major);
                var compatN = compat.Select(NormalizeBrand).ToArray();

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
                        sbyte v => v, byte v => v, short v => v, ushort v => v,
                        int v => v, uint v => v, long v => v, ulong v => v,
                        float v => v, double v => v, decimal v => (double)v,
                        _ => 0d
                    };

                    var w = ToDouble(wObj);
                    var h = ToDouble(hObj);
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
        => DetectMediaKind(path) is MediaKind.Image or MediaKind.Video or MediaKind.Audio;

    public static bool IsImageFileFast(string path)
        => DetectMediaKind(path) == MediaKind.Image;

    public static string ReadExifUniqueImageID(string imagePath)
    {
        if (!IsImageFileFast(imagePath))
            return "";

        try
        {
            var directories = ImageMetadataReader.ReadMetadata(imagePath);
            var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            var imageID = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagImageUniqueId);
            return imageID ?? "";
        }
        catch (Exception ex)
        {
            PhotoMoveYearMonthFolder.Logger.LogError("Exception ReadExifUniqueImageID " + ex.Message + " " + imagePath);
            return "";
        }
    }

    // Metadati condizionati al tipo. Per MP4/MOV usa QuickTime. Fallback alla data del filesystem.
    public static string ReadExifDataOrFileSystemDate(string mediaPath)
    {
        const string defaultDate = "19700101";
        try
        {
            var kind = DetectMediaKind(mediaPath);
            var directories = ImageMetadataReader.ReadMetadata(mediaPath);

            if (kind == MediaKind.Image)
            {
                var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                var ifd0Directory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();

                DateTime? datePictureTaken = subIfdDirectory?.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal)
                    ?? subIfdDirectory?.GetDateTime(ExifDirectoryBase.TagDateTimeDigitized)
                    ?? ifd0Directory?.GetDateTime(ExifDirectoryBase.TagDateTime);

                if (datePictureTaken != null)
                    return datePictureTaken.Value.ToString("yyyyMMdd");
            }
            else if (kind == MediaKind.Video)
            {
                // Molti MP4/QuickTime espongono i metadati come QuickTime
                var qt = directories.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();
                DateTime? dt = qt?.GetDateTime(QuickTimeMovieHeaderDirectory.TagCreated);
                if (dt != null)
                    return dt.Value.ToString("yyyyMMdd");
            }
        }
        catch (Exception ex)
        {
            PhotoMoveYearMonthFolder.Logger.LogError("Exception ReadExifData " + ex.Message + " " + mediaPath);
        }

        try
        {
            var dt = File.GetCreationTime(mediaPath);
            return dt.ToString("yyyyMMdd");
        }
        catch
        {
            return defaultDate;
        }
    }

    public static string GenerateNewFileName(string filePath)
    {
        string directory = Path.GetDirectoryName(filePath) ?? throw new ArgumentNullException(nameof(filePath));
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        string fileExtension = Path.GetExtension(filePath);
        int counter = 1;
        string newFileName = $"{fileNameWithoutExtension}_{counter}{fileExtension}";
        while (File.Exists(Path.Combine(directory, newFileName)))
        {
            counter++;
            newFileName = $"{fileNameWithoutExtension}_{counter}{fileExtension}";
        }
        return Path.Combine(directory, newFileName);
    }
}