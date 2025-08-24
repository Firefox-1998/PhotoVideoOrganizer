using System.Reflection;

namespace PhotoMoveYearMonthFolder
{
    public partial class FrmAbout : Form
    {
        public FrmAbout()
        {
            InitializeComponent();
            ApplyInfo();
        }

        private void ApplyInfo()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "";
            var version = assembly.GetName().Version?.ToString() ?? "";
            var developer = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(a => a.Key == "Developer")?.Value ?? "Unknown Developer";
            var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright ?? "Copyright ©";


            lblTitle.Text = $"📸 {Application.ProductName} - About";
            lblVersion.Text = $"🛠️ Version: {version}";
            lblAuthor.Text = $"👨‍💻 Developer: {developer}";
            lblCopyright.Text = $"{copyright}";
            lblLicense.Text = $"📄 License: MIT";
            lblDescription.Text = $"📝 {description}";
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}