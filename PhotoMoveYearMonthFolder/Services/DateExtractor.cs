using System.Globalization;
using System.Text.RegularExpressions;

namespace PhotoMoveYearMonthFolder.Services
{
    /// <summary>
    /// Extracts date information from file names and EXIF metadata.
    /// Supports multiple date formats and fallback strategies.
    /// </summary>
    public sealed partial class DateExtractor
    {
        #region Constants

        public const string DefaultYear = "1970";
        public const string DefaultMonth = "01";
        public const string InvalidYear = "9999";
        public const string InvalidMonth = "99";
        private const int MinValidYear = 1970;

        #endregion

        #region Static Data

        private static readonly Dictionary<string, int> PrefixToIndexMap = new()
        {
            { "IMG-", 4 },
            { "IMG_", 4 },
            { "VID-", 4 },
            { "AUD-", 4 },
            { "PPT-", 4 },
            { "Screenshot_", 11 },
            { "VideoCapture_", 13 },
            { "IMG", 3 },
            { "VID", 3 },
            { "WP_", 3 },
        };

        private static readonly string[] DateFormats =
        [
            // 8-digit formats
            "yyyyMMdd", "ddMMyyyy", "MMddyyyy",
            // Formats with dashes
            "yyyy-MM-dd", "dd-MM-yyyy", "MM-dd-yyyy",
            // 6-digit formats
            "yyyyMM", "MMyyyy",
            // 6-digit formats with dash
            "MM-yyyy"
        ];

        #endregion

        #region Generated Regex

        [GeneratedRegex(@"\b(\d{8}|\d{6}|\d{4}-\d{2}-\d{2}|\d{2}-\d{2}-\d{4}|\d{4}-\d{2}|\d{2}-\d{4})\b", RegexOptions.CultureInvariant)]
        private static partial Regex DatePatternRegex();

        #endregion

        #region Public Methods

        /// <summary>
        /// Extracts year and month from a file using multiple strategies:
        /// 1. Known prefix patterns (IMG_, VID_, etc.)
        /// 2. Regex-based date detection in filename
        /// 3. EXIF metadata
        /// 4. File system date as fallback
        /// </summary>
        public (string Year, string Month) ExtractYearMonth(string fileName, string filePath)
        {
            // Strategy 1: Try prefix-based extraction
            (string year, string month, bool found) = TryExtractFromPrefix(fileName);
            if (found)
            {
                return (year, month);
            }

            // Strategy 2: Regex-based extraction from filename
            (year, month) = ExtractFromRegex(fileName);
            if (IsValidYear(year))
            {
                return (year, month);
            }

            // Strategy 3 & 4: EXIF or filesystem fallback
            return ExtractFromExifOrFileSystem(filePath);
        }

        /// <summary>
        /// Extracts year and month using only regex pattern matching.
        /// Does not access the file system.
        /// </summary>
        public static (string Year, string Month) ExtractFromRegex(string fileName)
        {
            MatchCollection matches = DatePatternRegex().Matches(fileName);

            foreach (Match match in matches)
            {
                foreach (string format in DateFormats)
                {
                    if (DateTime.TryParseExact(
                        match.Value,
                        format,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime date))
                    {
                        string year = date.ToString("yyyy");
                        if (IsValidYear(year))
                        {
                            return (year, date.ToString("MM"));
                        }
                    }
                }
            }

            return (InvalidYear, InvalidMonth);
        }

        #endregion

        #region Private Methods

        private static (string Year, string Month, bool Found) TryExtractFromPrefix(string fileName)
        {
            foreach (KeyValuePair<string, int> entry in PrefixToIndexMap)
            {
                if (!fileName.StartsWith(entry.Key, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                int startIndex = entry.Value;
                if (fileName.Length < startIndex + 6)
                {
                    continue;
                }

                string possibleYear = fileName[startIndex..(startIndex + 4)];
                string possibleMonth = fileName[(startIndex + 4)..(startIndex + 6)];

                if (IsValidYear(possibleYear))
                {
                    return (possibleYear, possibleMonth, true);
                }
            }

            return (DefaultYear, DefaultMonth, false);
        }

        private static (string Year, string Month) ExtractFromExifOrFileSystem(string filePath)
        {
            string parsedDate = MediaFileHelper.ReadExifDataOrFileSystemDate(filePath);

            if (parsedDate.Length >= 6)
            {
                string year = parsedDate[..4];
                string month = parsedDate[4..6];

                if (IsValidYear(year))
                {
                    return (year, month);
                }
            }

            return (DefaultYear, DefaultMonth);
        }

        private static bool IsValidYear(string yearString)
        {
            return int.TryParse(yearString, out int year)
                   && year >= MinValidYear
                   && year <= DateTime.Now.Year;
        }

        #endregion
    }
}