using ExifLib;
using System.Globalization;
using System.Security.Cryptography;

internal static class frmPhotoSearchMoveHelpers
{

    public static async Task CopyFileAsync(string sourceFile, string destinationFile)
    {
        FileStream sourceStream = new(sourceFile, FileMode.Open, FileAccess.Read);
        var destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write);
        await sourceStream.CopyToAsync(destinationStream);
    }

    public static bool FilesAreIdentical(string path1, string path2)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] hash1 = GetFileHash(sha256, path1);
        byte[] hash2 = GetFileHash(sha256, path2);

        for (int i = 0; i < hash1.Length; i++)
        {
            if (hash1[i] != hash2[i])
            {
                return false;
            }
        }

        return true;
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

    private static byte[] GetFileHash(SHA256 sha256, string path)
    {
        FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return sha256.ComputeHash(stream);
    }

    public static bool IsValidFile(string file)
    {
        // Ottieni l'estensione del file
        string extension = Path.GetExtension(file).ToLower();

        // Elenco di estensioni di file immagine
        string[] validExtensions = [".jpg", ".jpeg", ".png", ".bmp", ".gif", ".mp4"];

        // Restituisce true se l'estensione è presente nell'elenco
        return validExtensions.Contains(extension);
    }

    public static void ReadExifData(string image, out DateTime parsedDate)
    {
        try
        {
            var reader = new ExifReader(image);

            reader.GetTagValue(ExifTags.DateTime, out DateTime date);
            bool isDate = DateTime.TryParse(date.ToString(), provider: CultureInfo.InvariantCulture,
                                        DateTimeStyles.None,
                                        out _);

            if (isDate)
            {
                parsedDate = DateTime.ParseExact(date.ToString(),
                                                     "yyyyMMdd",
                                                     provider: CultureInfo.InvariantCulture,
                                                     style: DateTimeStyles.None);
            }
            else
            {
                reader.GetTagValue(ExifTags.DateTimeOriginal, out DateTime dateoriginal);
                isDate = DateTime.TryParse(dateoriginal.ToString(), provider: CultureInfo.InvariantCulture,
                                        DateTimeStyles.None,
                                        out _);
                if (isDate)
                {
                    parsedDate = DateTime.ParseExact(dateoriginal.ToString(),
                                                     "yyyyMMdd",
                                                     provider: CultureInfo.InvariantCulture,
                                                     style: DateTimeStyles.None);

                }
                else
                {
                    parsedDate = DateTime.ParseExact("19700101",
                                                     "yyyyMMdd",
                                                     provider: CultureInfo.InvariantCulture,
                                                     style: DateTimeStyles.None);
                }
            }
        }
        catch (ExifLibException)
        {
            parsedDate = DateTime.ParseExact("19700101",
                                             "yyyyMMdd",
                                             provider: CultureInfo.InvariantCulture,
                                             style: DateTimeStyles.None);
            return;
        }
    }
}