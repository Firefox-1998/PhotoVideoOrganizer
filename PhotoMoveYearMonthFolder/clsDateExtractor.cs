using System.Globalization;

namespace PhotoMoveYearMonthFolder
{
    public static class clsDateExtractor
    {
        // Lista di tutti i formati di data possibili
        private static readonly List<string> Formats = new List<string>
    {
        "yyyyMMdd",
        "yyyy-MM-dd",
        "ddMMyyyy",
        "dd-MM-yyyy",
        "MMddyyyy",
        "MM-dd-yyyy",
        "yyyyMM",
        "yyyy-MM",
        "MMyyyy",
        "MM-yyyy"
    };

        public static (string Year, string Month) ExtractYearMonth(string fileName)
        {
            // Cerca ogni formato nella stringa
            foreach (var format in Formats)
            {
                for (int i = 0; i <= fileName.Length - format.Length; i++)
                {
                    var substring = fileName.Substring(i, format.Length);
                    if (DateTime.TryParseExact(substring, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                    {
                        return (date.ToString("yyyy"), date.ToString("MM"));
                    }
                }
            }

            // Se nessun formato corrisponde, restituisci null
            return ("9999", "99");
        }
    }
}
