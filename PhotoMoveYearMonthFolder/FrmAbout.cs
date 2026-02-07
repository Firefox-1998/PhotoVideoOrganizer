using PhotoMoveYearMonthFolder.Resources;
using System.Reflection;

namespace PhotoMoveYearMonthFolder
{
    public partial class FrmAbout : Form
    {
        public FrmAbout()
        {
            InitializeComponent();

            // Subscribe to global event to update texts when culture changes
            LocalizationManager.CultureChanged += LocalizationManager_CultureChanged;

            // Apply initial texts and info (evaluated with current CurrentUICulture)
            ApplyInfo();
        }

        private void LocalizationManager_CultureChanged(object? sender, EventArgs e)
        {
            // If called from a different thread, use Invoke to update the UI
            if (InvokeRequired)
            {
                Invoke((Action)ApplyInfo);
            }
            else
            {
                ApplyInfo();
            }
        }

        private void ApplyInfo()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var description = About.Apply_Description; // Localized description from About.resx
            var version = assembly.GetName().Version?.ToString() ?? "";
            var developer = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(a => a.Key == "Developer")?.Value ?? About.ApplyInfo_MissingDeveloper;
            var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? About.ApplyInfo_MissingCopyright;

            // Localized texts from strongly-typed resources (About.resx)
            lblTitle.Text = string.Format(About.ApplyInfo_ApplicationProductNameAbout, Application.ProductName);
            lblVersion.Text = string.Format(About.ApplyInfo_Version, version);
            lblAuthor.Text = string.Format(About.ApplyInfo_Developer, developer);
            lblCopyright.Text = $"{copyright}";
            lblLicense.Text = About.ApplyInfo_License;
            lblDescription.Text = $"📝 {description}";
            btnClose.Text = About.ApplyInfo_Close;
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Unsubscribe from event to avoid memory leak
                LocalizationManager.CultureChanged -= LocalizationManager_CultureChanged;
            }
            base.Dispose(disposing);
        }
    }
}