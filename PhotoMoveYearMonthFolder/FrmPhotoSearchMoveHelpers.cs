using ExifLib;
using PhotoMoveYearMonthFolder;
using System.Security.Cryptography;

internal static partial class FrmPhotoSearchMoveHelpers
{
    public static async Task CopyFileAsync(string sourceFile, string destinationFile)
    {
        FileStream sourceStream = new(sourceFile, FileMode.Open, FileAccess.Read);
        var destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write);
        await sourceStream.CopyToAsync(destinationStream);
    }
    public static string ComputeHash(string file)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] hash1 = GetFileHash(sha256, file);
        return BitConverter.ToString(value: hash1).Replace(" - ", string.Empty);
    }
    private static byte[] GetFileHash(SHA256 sha256, string path)
    {
        FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return sha256.ComputeHash(stream);
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
    public static bool IsValidFile(string file)
    {
        // Ottieni l'estensione del file
        string extension = Path.GetExtension(file).ToLower();

        // Elenco di estensioni di file immagine
        string[] validExtensions = [".jpg", ".jpeg", ".png", ".bmp", ".gif", ".mp4", ".mkv", ".opus", ".mp3", ".m4a", ".oga"];

        // Restituisce true se l'estensione è presente nell'elenco
        return validExtensions.Contains(extension);
    }
    public static string ReadExifUniqueImageID(string imagePath)
    {
        try
        {
            using ExifReader reader = new(imagePath);
            // Estrai il tag Image ID
            if (reader.GetTagValue(ExifTags.ImageUniqueID, out string imageID))
            {
                return imageID;
            }
            else
            {
                Logger.Log("Unique ImageID non presente " + imagePath);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Eccezione ReadExifData " + ex.Message + " " + imagePath);
        }
        return "";
    }

    public static string ReadExifData(string imagePath)
    {
        string defaultDate = "19700101";
        try
        {
            using ExifReader reader = new(imagePath);

            if (reader.GetTagValue(ExifTags.DateTimeDigitized, out DateTime datePictureTaken) ||
                reader.GetTagValue(ExifTags.DateTimeOriginal, out datePictureTaken) ||
                reader.GetTagValue(ExifTags.DateTime, out datePictureTaken))
            {
                return datePictureTaken.ToString("yyyyMMdd");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Eccezione ReadExifData " + ex.Message + " " + imagePath);
        }
        return defaultDate;
    }
}