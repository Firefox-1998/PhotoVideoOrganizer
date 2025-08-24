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

        private void InitializeComponent()
        {
            lblTitle = new Label();
            lblVersion = new Label();
            lblAuthor = new Label();
            lblCopyright = new Label();
            lblLicense = new Label();
            lblDescription = new Label();
            btnClose = new Button();
            tableLayoutPanel = new TableLayoutPanel();
            tableLayoutPanel.SuspendLayout();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTitle.Location = new Point(23, 20);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(434, 70);
            lblTitle.TabIndex = 0;
            lblTitle.TextAlign = ContentAlignment.TopCenter;
            // 
            // lblVersion
            // 
            lblVersion.Dock = DockStyle.Fill;
            lblVersion.Font = new Font("Segoe UI", 12F);
            lblVersion.Location = new Point(23, 90);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(434, 32);
            lblVersion.TabIndex = 1;
            lblVersion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblAuthor
            // 
            lblAuthor.Dock = DockStyle.Fill;
            lblAuthor.Font = new Font("Segoe UI", 12F);
            lblAuthor.Location = new Point(23, 122);
            lblAuthor.Name = "lblAuthor";
            lblAuthor.Size = new Size(434, 32);
            lblAuthor.TabIndex = 2;
            lblAuthor.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblCopyright
            // 
            lblCopyright.Dock = DockStyle.Fill;
            lblCopyright.Font = new Font("Segoe UI", 12F);
            lblCopyright.Location = new Point(23, 154);
            lblCopyright.Name = "lblCopyright";
            lblCopyright.Size = new Size(434, 32);
            lblCopyright.TabIndex = 3;
            lblCopyright.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblLicense
            // 
            lblLicense.Dock = DockStyle.Fill;
            lblLicense.Font = new Font("Segoe UI", 12F);
            lblLicense.Location = new Point(23, 186);
            lblLicense.Name = "lblLicense";
            lblLicense.Size = new Size(434, 32);
            lblLicense.TabIndex = 4;
            lblLicense.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblDescription
            // 
            lblDescription.Dock = DockStyle.Fill;
            lblDescription.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblDescription.Location = new Point(23, 218);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(434, 32);
            lblDescription.TabIndex = 5;
            lblDescription.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnClose
            // 
            btnClose.Dock = DockStyle.Fill;
            btnClose.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnClose.Location = new Point(23, 253);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(434, 44);
            btnClose.TabIndex = 6;
            btnClose.Text = "✨ Close ✨";
            btnClose.Click += btnClose_Click;
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.ColumnCount = 1;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
            tableLayoutPanel.Controls.Add(lblTitle, 0, 0);
            tableLayoutPanel.Controls.Add(lblVersion, 0, 1);
            tableLayoutPanel.Controls.Add(lblAuthor, 0, 2);
            tableLayoutPanel.Controls.Add(lblCopyright, 0, 3);
            tableLayoutPanel.Controls.Add(lblLicense, 0, 4);
            tableLayoutPanel.Controls.Add(lblDescription, 0, 5);
            tableLayoutPanel.Controls.Add(btnClose, 0, 6);
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.Location = new Point(0, 0);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.Padding = new Padding(20);
            tableLayoutPanel.RowCount = 7;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel.Size = new Size(480, 320);
            tableLayoutPanel.TabIndex = 0;
            // 
            // FrmAbout
            // 
            BackColor = SystemColors.Control;
            ClientSize = new Size(480, 320);
            Controls.Add(tableLayoutPanel);
            Font = new Font("Segoe UI", 10F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Name = "FrmAbout";
            StartPosition = FormStartPosition.CenterParent;
            Text = "About";
            tableLayoutPanel.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}