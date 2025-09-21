using System.Globalization;
using System.Text.RegularExpressions;

namespace PhotoMoveYearMonthFolder
{
    public static partial class ClsDateExtractor
    {
        // Regex per trovare sequenze numeriche che potrebbero essere date.
        // Cerca numeri di 8 cifre, 6 cifre o formati con trattini.
        [GeneratedRegex(@"\b(\d{8}|\d{6}|\d{4}-\d{2}-\d{2}|\d{2}-\d{2}-\d{4}|\d{4}-\d{2}|\d{2}-\d{4})\b", RegexOptions.CultureInvariant)]
        private static partial Regex DateRegex();

        // Lista di formati di data da provare, ordinati per probabilità o specificità.
        private static readonly string[] Formats =
        [
            // Formati a 8 cifre
            "yyyyMMdd", "ddMMyyyy", "MMddyyyy",
            // Formati con trattini
            "yyyy-MM-dd", "dd-MM-yyyy", "MM-dd-yyyy",
            // Formati a 6 cifre
            "yyyyMM", "MMyyyy",
            // Formati a 6 cifre con trattino
            "MM-yyyy"
        ];

        public static (string Year, string Month) ExtractYearMonth(string fileName)
        {
            // Cerca tutte le corrispondenze della regex nel nome del file.
            MatchCollection matches = DateRegex().Matches(fileName);
            foreach (Match match in matches)
            {
                // Per ogni potenziale data trovata, prova a fare il parsing con i formati noti.
                foreach (string format in Formats)
                {
                    if (DateTime.TryParseExact(match.Value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                    {
                        // Appena viene trovata una data valida, restituisci anno e mese.
                        return (date.ToString("yyyy"), date.ToString("MM"));
                    }
                }
            }

            // Se nessuna corrispondenza valida viene trovata, restituisci i valori di fallback.
            return ("9999", "99");
        }
    }
}
