namespace PhotoMoveYearMonthFolder
{
    partial class FrmAbout
    {
        private Label lblTitle;
        private Label lblVersion;
        private Label lblAuthor;
        private Label lblCopyright;
        private Label lblLicense;
        private Label lblDescription;
        private Button btnClose;
        private TableLayoutPanel tableLayoutPanel;
        private Panel panelHeader;
        private Panel panelContent;

        private void InitializeComponent()
        {
            panelHeader = new Panel();
            lblTitle = new Label();
            panelContent = new Panel();
            tableLayoutPanel = new TableLayoutPanel();
            lblVersion = new Label();
            lblAuthor = new Label();
            lblCopyright = new Label();
            lblLicense = new Label();
            lblDescription = new Label();
            btnClose = new Button();
            panelHeader.SuspendLayout();
            panelContent.SuspendLayout();
            tableLayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(0, 120, 212);
            panelHeader.Controls.Add(lblTitle);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Padding = new Padding(20);
            panelHeader.Size = new Size(481, 80);
            panelHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 20);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(441, 40);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "📷 Photo/Video Organizer";
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // panelContent
            // 
            panelContent.BackColor = Color.FromArgb(250, 250, 250);
            panelContent.Controls.Add(tableLayoutPanel);
            panelContent.Dock = DockStyle.Fill;
            panelContent.Location = new Point(0, 80);
            panelContent.Name = "panelContent";
            panelContent.Padding = new Padding(24);
            panelContent.Size = new Size(481, 289);
            panelContent.TabIndex = 1;
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.BackColor = Color.White;
            tableLayoutPanel.ColumnCount = 1;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel.Controls.Add(lblVersion, 0, 0);
            tableLayoutPanel.Controls.Add(lblAuthor, 0, 1);
            tableLayoutPanel.Controls.Add(lblCopyright, 0, 2);
            tableLayoutPanel.Controls.Add(lblLicense, 0, 3);
            tableLayoutPanel.Controls.Add(lblDescription, 0, 4);
            tableLayoutPanel.Controls.Add(btnClose, 0, 5);
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.Location = new Point(24, 24);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.Padding = new Padding(16);
            tableLayoutPanel.RowCount = 6;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            tableLayoutPanel.Size = new Size(433, 241);
            tableLayoutPanel.TabIndex = 0;
            // 
            // lblVersion
            // 
            lblVersion.Dock = DockStyle.Fill;
            lblVersion.Font = new Font("Segoe UI", 10F);
            lblVersion.ForeColor = Color.FromArgb(64, 64, 64);
            lblVersion.Location = new Point(19, 16);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(395, 28);
            lblVersion.TabIndex = 1;
            lblVersion.Text = "📦 Version: ";
            lblVersion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblAuthor
            // 
            lblAuthor.Dock = DockStyle.Fill;
            lblAuthor.Font = new Font("Segoe UI", 10F);
            lblAuthor.ForeColor = Color.FromArgb(64, 64, 64);
            lblAuthor.Location = new Point(19, 44);
            lblAuthor.Name = "lblAuthor";
            lblAuthor.Size = new Size(395, 28);
            lblAuthor.TabIndex = 2;
            lblAuthor.Text = "👨‍💻 Developer: ";
            lblAuthor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblCopyright
            // 
            lblCopyright.Dock = DockStyle.Fill;
            lblCopyright.Font = new Font("Segoe UI", 10F);
            lblCopyright.ForeColor = Color.FromArgb(64, 64, 64);
            lblCopyright.Location = new Point(19, 72);
            lblCopyright.Name = "lblCopyright";
            lblCopyright.Size = new Size(395, 28);
            lblCopyright.TabIndex = 3;
            lblCopyright.Text = "©";
            lblCopyright.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblLicense
            // 
            lblLicense.Dock = DockStyle.Fill;
            lblLicense.Font = new Font("Segoe UI", 10F);
            lblLicense.ForeColor = Color.FromArgb(64, 64, 64);
            lblLicense.Location = new Point(19, 100);
            lblLicense.Name = "lblLicense";
            lblLicense.Size = new Size(395, 28);
            lblLicense.TabIndex = 4;
            lblLicense.Text = "📜 License: ";
            lblLicense.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblDescription
            // 
            lblDescription.Dock = DockStyle.Fill;
            lblDescription.Font = new Font("Segoe UI", 9F);
            lblDescription.ForeColor = Color.FromArgb(100, 100, 100);
            lblDescription.Location = new Point(19, 128);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(395, 53);
            lblDescription.TabIndex = 5;
            lblDescription.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnClose
            // 
            btnClose.BackColor = Color.FromArgb(0, 120, 212);
            btnClose.Cursor = Cursors.Hand;
            btnClose.Dock = DockStyle.Fill;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 100, 180);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnClose.ForeColor = Color.White;
            btnClose.Location = new Point(19, 184);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(395, 38);
            btnClose.TabIndex = 6;
            btnClose.Text = "✓ Close";
            btnClose.UseVisualStyleBackColor = false;
            btnClose.Click += BtnClose_Click;
            // 
            // FrmAbout
            // 
            BackColor = Color.FromArgb(243, 243, 243);
            ClientSize = new Size(481, 369);
            ControlBox = false;
            Controls.Add(panelContent);
            Controls.Add(panelHeader);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            Name = "FrmAbout";
            StartPosition = FormStartPosition.CenterParent;
            Text = "ℹ️ About";
            panelHeader.ResumeLayout(false);
            panelContent.ResumeLayout(false);
            tableLayoutPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}