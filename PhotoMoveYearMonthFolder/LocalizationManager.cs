using System.Globalization;

namespace PhotoMoveYearMonthFolder
{
    internal static class LocalizationManager
    {
        public static event EventHandler? CultureChanged;

        public static void SetCulture(string cultureName)
        {
            var ci = new CultureInfo(cultureName);

            // Imposta culture di default per i nuovi thread e per il thread corrente
            CultureInfo.DefaultThreadCurrentCulture = ci;
            CultureInfo.DefaultThreadCurrentUICulture = ci;
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            // Notifica tutte le form sottoscritte
            CultureChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void SetCultureByIndex(int index)
        {
            string culture = index switch
            {
                0 => "en-US",
                1 => "it-IT",
                2 => "fr-FR",
                3 => "de-DE",
                _ => "en-US"
            };

            SetCulture(culture);
        }
    }
}