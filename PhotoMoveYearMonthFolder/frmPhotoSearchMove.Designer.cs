namespace PhotoMoveYearMonthFolder
{
    partial class FrmPhotoSearchMove
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmPhotoSearchMove));
            Lbl_DirSearch = new Label();
            Btn_DirSearch = new Button();
            Lbl_DirDestination = new Label();
            Btn_DirDest = new Button();
            Btn_Start = new Button();
            Fbd_DirSel = new FolderBrowserDialog();
            LblNumFiles = new Label();
            LblFileProc = new Label();
            Btn_Cancel = new Button();
            pbProcessFiles = new ProgressBar();
            tbMaxThread = new TrackBar();
            lblMaxThread = new Label();
            pbProcessedOtherFiles = new ProgressBar();
            LblOtherFileProc = new Label();
            LblNumOtherFiles = new Label();
            Btn_Exit = new Button();
            lblComment = new Label();
            chkRootOnly = new CheckBox();
            cmbLanguage = new ComboBox();
            lblLanguageUI = new Label();
            panelHeader = new Panel();
            lblAppTitle = new Label();
            panelMain = new Panel();
            panelDirectories = new Panel();
            panelSettings = new Panel();
            panelProgress = new Panel();
            panelActions = new Panel();
            ((System.ComponentModel.ISupportInitialize)tbMaxThread).BeginInit();
            panelHeader.SuspendLayout();
            panelMain.SuspendLayout();
            panelDirectories.SuspendLayout();
            panelSettings.SuspendLayout();
            panelProgress.SuspendLayout();
            panelActions.SuspendLayout();
            SuspendLayout();
            // 
            // Lbl_DirSearch
            // 
            Lbl_DirSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Lbl_DirSearch.BackColor = Color.FromArgb(249, 249, 249);
            Lbl_DirSearch.BorderStyle = BorderStyle.FixedSingle;
            Lbl_DirSearch.Font = new Font("Segoe UI", 9F);
            Lbl_DirSearch.ForeColor = Color.FromArgb(64, 64, 64);
            Lbl_DirSearch.Location = new Point(19, 26);
            Lbl_DirSearch.Name = "Lbl_DirSearch";
            Lbl_DirSearch.Padding = new Padding(8, 0, 8, 0);
            Lbl_DirSearch.Size = new Size(360, 24);
            Lbl_DirSearch.TabIndex = 0;
            Lbl_DirSearch.Text = "DS";
            Lbl_DirSearch.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // Btn_DirSearch
            // 
            Btn_DirSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Btn_DirSearch.BackColor = Color.FromArgb(0, 120, 212);
            Btn_DirSearch.Cursor = Cursors.Hand;
            Btn_DirSearch.FlatAppearance.BorderSize = 0;
            Btn_DirSearch.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 100, 180);
            Btn_DirSearch.FlatStyle = FlatStyle.Flat;
            Btn_DirSearch.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            Btn_DirSearch.ForeColor = Color.White;
            Btn_DirSearch.Location = new Point(386, 9);
            Btn_DirSearch.Name = "Btn_DirSearch";
            Btn_DirSearch.Size = new Size(126, 58);
            Btn_DirSearch.TabIndex = 1;
            Btn_DirSearch.Text = "📂 SDTIS";
            Btn_DirSearch.UseVisualStyleBackColor = false;
            Btn_DirSearch.Click += Btn_DirSearch_Click;
            // 
            // Lbl_DirDestination
            // 
            Lbl_DirDestination.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Lbl_DirDestination.BackColor = Color.FromArgb(249, 249, 249);
            Lbl_DirDestination.BorderStyle = BorderStyle.FixedSingle;
            Lbl_DirDestination.Font = new Font("Segoe UI", 9F);
            Lbl_DirDestination.ForeColor = Color.FromArgb(64, 64, 64);
            Lbl_DirDestination.Location = new Point(19, 90);
            Lbl_DirDestination.Name = "Lbl_DirDestination";
            Lbl_DirDestination.Padding = new Padding(8, 0, 8, 0);
            Lbl_DirDestination.Size = new Size(360, 24);
            Lbl_DirDestination.TabIndex = 2;
            Lbl_DirDestination.Text = "DD";
            Lbl_DirDestination.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // Btn_DirDest
            // 
            Btn_DirDest.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Btn_DirDest.BackColor = Color.FromArgb(0, 120, 212);
            Btn_DirDest.Cursor = Cursors.Hand;
            Btn_DirDest.FlatAppearance.BorderSize = 0;
            Btn_DirDest.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 100, 180);
            Btn_DirDest.FlatStyle = FlatStyle.Flat;
            Btn_DirDest.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            Btn_DirDest.ForeColor = Color.White;
            Btn_DirDest.Location = new Point(386, 73);
            Btn_DirDest.Name = "Btn_DirDest";
            Btn_DirDest.Size = new Size(126, 58);
            Btn_DirDest.TabIndex = 3;
            Btn_DirDest.Text = "📁 SDTIC";
            Btn_DirDest.UseVisualStyleBackColor = false;
            Btn_DirDest.Click += Btn_DirDest_Click;
            // 
            // Btn_Start
            // 
            Btn_Start.BackColor = Color.FromArgb(16, 124, 16);
            Btn_Start.Cursor = Cursors.Hand;
            Btn_Start.FlatAppearance.BorderSize = 0;
            Btn_Start.FlatAppearance.MouseOverBackColor = Color.FromArgb(14, 100, 14);
            Btn_Start.FlatStyle = FlatStyle.Flat;
            Btn_Start.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            Btn_Start.ForeColor = Color.White;
            Btn_Start.Location = new Point(2, 0);
            Btn_Start.Name = "Btn_Start";
            Btn_Start.Size = new Size(110, 38);
            Btn_Start.TabIndex = 4;
            Btn_Start.Text = "▶ St";
            Btn_Start.UseVisualStyleBackColor = false;
            Btn_Start.Click += Btn_Start_Click;
            // 
            // Fbd_DirSel
            // 
            Fbd_DirSel.SelectedPath = "C:\\";
            // 
            // LblNumFiles
            // 
            LblNumFiles.Font = new Font("Segoe UI", 9F);
            LblNumFiles.ForeColor = Color.FromArgb(64, 64, 64);
            LblNumFiles.Location = new Point(16, 12);
            LblNumFiles.Name = "LblNumFiles";
            LblNumFiles.Size = new Size(180, 18);
            LblNumFiles.TabIndex = 5;
            LblNumFiles.Text = "-";
            // 
            // LblFileProc
            // 
            LblFileProc.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            LblFileProc.Font = new Font("Segoe UI", 9F);
            LblFileProc.ForeColor = Color.FromArgb(100, 100, 100);
            LblFileProc.Location = new Point(200, 12);
            LblFileProc.Name = "LblFileProc";
            LblFileProc.Size = new Size(312, 18);
            LblFileProc.TabIndex = 6;
            LblFileProc.Text = "-";
            LblFileProc.TextAlign = ContentAlignment.TopRight;
            // 
            // Btn_Cancel
            // 
            Btn_Cancel.BackColor = Color.FromArgb(196, 43, 28);
            Btn_Cancel.Cursor = Cursors.Hand;
            Btn_Cancel.Enabled = false;
            Btn_Cancel.FlatAppearance.BorderSize = 0;
            Btn_Cancel.FlatAppearance.MouseOverBackColor = Color.FromArgb(160, 35, 22);
            Btn_Cancel.FlatStyle = FlatStyle.Flat;
            Btn_Cancel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            Btn_Cancel.ForeColor = Color.White;
            Btn_Cancel.Location = new Point(118, 0);
            Btn_Cancel.Name = "Btn_Cancel";
            Btn_Cancel.Size = new Size(291, 38);
            Btn_Cancel.TabIndex = 7;
            Btn_Cancel.Text = "⏹ Ca";
            Btn_Cancel.UseVisualStyleBackColor = false;
            Btn_Cancel.Click += Btn_Cancel_Click;
            // 
            // pbProcessFiles
            // 
            pbProcessFiles.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pbProcessFiles.Location = new Point(16, 32);
            pbProcessFiles.Name = "pbProcessFiles";
            pbProcessFiles.Size = new Size(496, 20);
            pbProcessFiles.Step = 1;
            pbProcessFiles.Style = ProgressBarStyle.Continuous;
            pbProcessFiles.TabIndex = 8;
            // 
            // tbMaxThread
            // 
            tbMaxThread.Location = new Point(16, 28);
            tbMaxThread.Maximum = 20;
            tbMaxThread.Minimum = 1;
            tbMaxThread.Name = "tbMaxThread";
            tbMaxThread.Size = new Size(200, 45);
            tbMaxThread.TabIndex = 9;
            tbMaxThread.Value = 4;
            tbMaxThread.Scroll += TbMaxThread_Scroll;
            // 
            // lblMaxThread
            // 
            lblMaxThread.BackColor = Color.Transparent;
            lblMaxThread.Font = new Font("Segoe UI", 9F);
            lblMaxThread.ForeColor = Color.FromArgb(64, 64, 64);
            lblMaxThread.Location = new Point(16, 5);
            lblMaxThread.Name = "lblMaxThread";
            lblMaxThread.Size = new Size(160, 20);
            lblMaxThread.TabIndex = 10;
            lblMaxThread.Text = "MT";
            // 
            // pbProcessedOtherFiles
            // 
            pbProcessedOtherFiles.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pbProcessedOtherFiles.Location = new Point(16, 90);
            pbProcessedOtherFiles.Name = "pbProcessedOtherFiles";
            pbProcessedOtherFiles.Size = new Size(496, 20);
            pbProcessedOtherFiles.Step = 1;
            pbProcessedOtherFiles.Style = ProgressBarStyle.Continuous;
            pbProcessedOtherFiles.TabIndex = 11;
            // 
            // LblOtherFileProc
            // 
            LblOtherFileProc.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            LblOtherFileProc.Font = new Font("Segoe UI", 9F);
            LblOtherFileProc.ForeColor = Color.FromArgb(100, 100, 100);
            LblOtherFileProc.Location = new Point(200, 70);
            LblOtherFileProc.Name = "LblOtherFileProc";
            LblOtherFileProc.Size = new Size(312, 18);
            LblOtherFileProc.TabIndex = 13;
            LblOtherFileProc.Text = "-";
            LblOtherFileProc.TextAlign = ContentAlignment.TopRight;
            // 
            // LblNumOtherFiles
            // 
            LblNumOtherFiles.Font = new Font("Segoe UI", 9F);
            LblNumOtherFiles.ForeColor = Color.FromArgb(64, 64, 64);
            LblNumOtherFiles.Location = new Point(16, 70);
            LblNumOtherFiles.Name = "LblNumOtherFiles";
            LblNumOtherFiles.Size = new Size(180, 18);
            LblNumOtherFiles.TabIndex = 12;
            LblNumOtherFiles.Text = "-";
            // 
            // Btn_Exit
            // 
            Btn_Exit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Btn_Exit.BackColor = Color.FromArgb(96, 96, 96);
            Btn_Exit.Cursor = Cursors.Hand;
            Btn_Exit.FlatAppearance.BorderSize = 0;
            Btn_Exit.FlatAppearance.MouseOverBackColor = Color.FromArgb(72, 72, 72);
            Btn_Exit.FlatStyle = FlatStyle.Flat;
            Btn_Exit.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            Btn_Exit.ForeColor = Color.White;
            Btn_Exit.Location = new Point(415, 0);
            Btn_Exit.Name = "Btn_Exit";
            Btn_Exit.Size = new Size(110, 38);
            Btn_Exit.TabIndex = 14;
            Btn_Exit.Text = "✕ E";
            Btn_Exit.UseVisualStyleBackColor = false;
            Btn_Exit.Click += Btn_Exit_Click;
            // 
            // lblComment
            // 
            lblComment.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblComment.Cursor = Cursors.Hand;
            lblComment.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point, 0);
            lblComment.ForeColor = Color.FromArgb(0, 120, 212);
            lblComment.Location = new Point(16, 424);
            lblComment.Name = "lblComment";
            lblComment.Size = new Size(528, 20);
            lblComment.TabIndex = 15;
            lblComment.Text = "PV";
            lblComment.TextAlign = ContentAlignment.MiddleCenter;
            lblComment.Click += LblComment_Click;
            // 
            // chkRootOnly
            // 
            chkRootOnly.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            chkRootOnly.Font = new Font("Segoe UI", 9F);
            chkRootOnly.ForeColor = Color.FromArgb(64, 64, 64);
            chkRootOnly.Location = new Point(236, 37);
            chkRootOnly.Name = "chkRootOnly";
            chkRootOnly.Size = new Size(273, 24);
            chkRootOnly.TabIndex = 16;
            chkRootOnly.Text = "SISRO";
            chkRootOnly.UseVisualStyleBackColor = true;
            // 
            // cmbLanguage
            // 
            cmbLanguage.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cmbLanguage.BackColor = Color.White;
            cmbLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbLanguage.FlatStyle = FlatStyle.Flat;
            cmbLanguage.Font = new Font("Segoe UI", 10F);
            cmbLanguage.FormattingEnabled = true;
            cmbLanguage.Items.AddRange(new object[] { "English (US)", "Italiano", "Français", "Deutsch" });
            cmbLanguage.Location = new Point(402, 18);
            cmbLanguage.Name = "cmbLanguage";
            cmbLanguage.Size = new Size(146, 25);
            cmbLanguage.TabIndex = 17;
            cmbLanguage.SelectedIndexChanged += CmbLanguage_SelectedIndexChanged;
            // 
            // lblLanguageUI
            // 
            lblLanguageUI.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblLanguageUI.Font = new Font("Segoe UI", 10F);
            lblLanguageUI.ForeColor = Color.White;
            lblLanguageUI.Location = new Point(259, 18);
            lblLanguageUI.Name = "lblLanguageUI";
            lblLanguageUI.Size = new Size(142, 22);
            lblLanguageUI.TabIndex = 18;
            lblLanguageUI.Text = "UI:";
            lblLanguageUI.TextAlign = ContentAlignment.MiddleRight;
            // 
            // panelHeader
            // 
            panelHeader.BackColor = Color.FromArgb(0, 120, 212);
            panelHeader.Controls.Add(lblAppTitle);
            panelHeader.Controls.Add(lblLanguageUI);
            panelHeader.Controls.Add(cmbLanguage);
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Location = new Point(0, 0);
            panelHeader.Name = "panelHeader";
            panelHeader.Padding = new Padding(16, 12, 16, 12);
            panelHeader.Size = new Size(560, 60);
            panelHeader.TabIndex = 100;
            // 
            // lblAppTitle
            // 
            lblAppTitle.AutoSize = true;
            lblAppTitle.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            lblAppTitle.ForeColor = Color.White;
            lblAppTitle.Location = new Point(10, 16);
            lblAppTitle.Name = "lblAppTitle";
            lblAppTitle.Size = new Size(236, 25);
            lblAppTitle.TabIndex = 0;
            lblAppTitle.Text = "📷 Photo/Video Organizer";
            // 
            // panelMain
            // 
            panelMain.BackColor = Color.FromArgb(243, 243, 243);
            panelMain.Controls.Add(panelDirectories);
            panelMain.Controls.Add(panelSettings);
            panelMain.Controls.Add(panelProgress);
            panelMain.Controls.Add(lblComment);
            panelMain.Controls.Add(panelActions);
            panelMain.Dock = DockStyle.Fill;
            panelMain.Location = new Point(0, 60);
            panelMain.Name = "panelMain";
            panelMain.Padding = new Padding(16);
            panelMain.Size = new Size(560, 451);
            panelMain.TabIndex = 101;
            // 
            // panelDirectories
            // 
            panelDirectories.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelDirectories.BackColor = Color.White;
            panelDirectories.Controls.Add(Lbl_DirSearch);
            panelDirectories.Controls.Add(Btn_DirSearch);
            panelDirectories.Controls.Add(Lbl_DirDestination);
            panelDirectories.Controls.Add(Btn_DirDest);
            panelDirectories.Location = new Point(16, 6);
            panelDirectories.Name = "panelDirectories";
            panelDirectories.Padding = new Padding(16);
            panelDirectories.Size = new Size(528, 142);
            panelDirectories.TabIndex = 0;
            // 
            // panelSettings
            // 
            panelSettings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelSettings.BackColor = Color.White;
            panelSettings.Controls.Add(lblMaxThread);
            panelSettings.Controls.Add(tbMaxThread);
            panelSettings.Controls.Add(chkRootOnly);
            panelSettings.Location = new Point(16, 154);
            panelSettings.Name = "panelSettings";
            panelSettings.Padding = new Padding(16);
            panelSettings.Size = new Size(528, 80);
            panelSettings.TabIndex = 1;
            // 
            // panelProgress
            // 
            panelProgress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelProgress.BackColor = Color.White;
            panelProgress.Controls.Add(LblNumFiles);
            panelProgress.Controls.Add(LblFileProc);
            panelProgress.Controls.Add(pbProcessFiles);
            panelProgress.Controls.Add(LblNumOtherFiles);
            panelProgress.Controls.Add(LblOtherFileProc);
            panelProgress.Controls.Add(pbProcessedOtherFiles);
            panelProgress.Location = new Point(16, 242);
            panelProgress.Name = "panelProgress";
            panelProgress.Padding = new Padding(16);
            panelProgress.Size = new Size(528, 130);
            panelProgress.TabIndex = 2;
            // 
            // panelActions
            // 
            panelActions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panelActions.BackColor = Color.Transparent;
            panelActions.Controls.Add(Btn_Start);
            panelActions.Controls.Add(Btn_Cancel);
            panelActions.Controls.Add(Btn_Exit);
            panelActions.Location = new Point(16, 378);
            panelActions.Name = "panelActions";
            panelActions.Size = new Size(528, 40);
            panelActions.TabIndex = 3;
            // 
            // FrmPhotoSearchMove
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(243, 243, 243);
            ClientSize = new Size(560, 511);
            Controls.Add(panelMain);
            Controls.Add(panelHeader);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "FrmPhotoSearchMove";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Photo/Video Organizer";
            FormClosing += FrmPhotoSearchMove_FormClosing;
            ((System.ComponentModel.ISupportInitialize)tbMaxThread).EndInit();
            panelHeader.ResumeLayout(false);
            panelHeader.PerformLayout();
            panelMain.ResumeLayout(false);
            panelDirectories.ResumeLayout(false);
            panelSettings.ResumeLayout(false);
            panelSettings.PerformLayout();
            panelProgress.ResumeLayout(false);
            panelActions.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panelHeader;
        private Label lblAppTitle;
        private Panel panelMain;
        private Panel panelDirectories;
        private Panel panelSettings;
        private Panel panelProgress;
        private Panel panelActions;
        private Label Lbl_DirSearch;
        private Button Btn_DirSearch;
        private Label Lbl_DirDestination;
        private Button Btn_DirDest;
        private Button Btn_Start;
        private FolderBrowserDialog Fbd_DirSel;
        private Label LblNumFiles;
        private Label LblFileProc;
        private Button Btn_Cancel;
        private ProgressBar pbProcessFiles;
        private TrackBar tbMaxThread;
        private Label lblMaxThread;
        private ProgressBar pbProcessedOtherFiles;
        private Label LblOtherFileProc;
        private Label LblNumOtherFiles;
        private Button Btn_Exit;
        private Label lblComment;
        private CheckBox chkRootOnly;
        private ComboBox cmbLanguage;
        private Label lblLanguageUI;
    }
}
