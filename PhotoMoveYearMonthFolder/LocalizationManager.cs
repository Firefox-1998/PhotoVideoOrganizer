using System.Globalization;

namespace PhotoMoveYearMonthFolder
{
    internal static class LocalizationManager
    {
        public static event EventHandler? CultureChanged;

        public static void SetCulture(string cultureName)
        {
            var ci = new CultureInfo(cultureName);

            // Set default culture for new threads and for the current thread
            CultureInfo.DefaultThreadCurrentCulture = ci;
            CultureInfo.DefaultThreadCurrentUICulture = ci;
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            // Notify all subscribed forms
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