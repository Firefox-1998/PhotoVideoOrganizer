namespace PhotoMoveYearMonthFolder
{
    partial class FrmPhotoSearchMove
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

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
            SuspendLayout();
            // 
            // Lbl_DirSearch
            // 
            Lbl_DirSearch.BorderStyle = BorderStyle.FixedSingle;
            Lbl_DirSearch.Location = new Point(12, 24);
            Lbl_DirSearch.Name = "Lbl_DirSearch";
            Lbl_DirSearch.Size = new Size(327, 18);
            Lbl_DirSearch.TabIndex = 0;
            Lbl_DirSearch.Text = "Directory Search";
            // 
            // Btn_DirSearch
            // 
            Btn_DirSearch.Location = new Point(360, 12);
            Btn_DirSearch.Name = "Btn_DirSearch";
            Btn_DirSearch.Size = new Size(102, 44);
            Btn_DirSearch.TabIndex = 1;
            Btn_DirSearch.Text = "Select Directory To Image Search";
            Btn_DirSearch.UseVisualStyleBackColor = true;
            Btn_DirSearch.Click += Btn_DirSearch_Click;
            // 
            // Lbl_DirDestination
            // 
            Lbl_DirDestination.BorderStyle = BorderStyle.FixedSingle;
            Lbl_DirDestination.Location = new Point(12, 73);
            Lbl_DirDestination.Name = "Lbl_DirDestination";
            Lbl_DirDestination.Size = new Size(327, 18);
            Lbl_DirDestination.TabIndex = 2;
            Lbl_DirDestination.Text = "Directory Destination";
            // 
            // Btn_DirDest
            // 
            Btn_DirDest.Location = new Point(360, 59);
            Btn_DirDest.Name = "Btn_DirDest";
            Btn_DirDest.Size = new Size(102, 44);
            Btn_DirDest.TabIndex = 3;
            Btn_DirDest.Text = "Select Directory To Image Copy";
            Btn_DirDest.UseVisualStyleBackColor = true;
            Btn_DirDest.Click += Btn_DirDest_Click;
            // 
            // Btn_Start
            // 
            Btn_Start.Location = new Point(360, 111);
            Btn_Start.Name = "Btn_Start";
            Btn_Start.Size = new Size(102, 44);
            Btn_Start.TabIndex = 4;
            Btn_Start.Text = "Start";
            Btn_Start.UseVisualStyleBackColor = true;
            Btn_Start.Click += Btn_Start_Click;
            // 
            // Fbd_DirSel
            // 
            Fbd_DirSel.SelectedPath = "C:\\";
            // 
            // LblNumFiles
            // 
            LblNumFiles.AutoSize = true;
            LblNumFiles.Location = new Point(12, 190);
            LblNumFiles.Name = "LblNumFiles";
            LblNumFiles.Size = new Size(12, 15);
            LblNumFiles.TabIndex = 5;
            LblNumFiles.Text = "-";
            // 
            // LblFileProc
            // 
            LblFileProc.AutoSize = true;
            LblFileProc.Location = new Point(192, 190);
            LblFileProc.Name = "LblFileProc";
            LblFileProc.Size = new Size(12, 15);
            LblFileProc.TabIndex = 6;
            LblFileProc.Text = "-";
            // 
            // Btn_Cancel
            // 
            Btn_Cancel.Enabled = false;
            Btn_Cancel.Location = new Point(360, 161);
            Btn_Cancel.Name = "Btn_Cancel";
            Btn_Cancel.Size = new Size(102, 44);
            Btn_Cancel.TabIndex = 7;
            Btn_Cancel.Text = "Cancel";
            Btn_Cancel.UseVisualStyleBackColor = true;
            Btn_Cancel.Click += Btn_Cancel_Click;
            // 
            // pbProcessFiles
            // 
            pbProcessFiles.Location = new Point(10, 218);
            pbProcessFiles.Name = "pbProcessFiles";
            pbProcessFiles.Size = new Size(452, 23);
            pbProcessFiles.Step = 1;
            pbProcessFiles.Style = ProgressBarStyle.Continuous;
            pbProcessFiles.TabIndex = 8;
            // 
            // FrmPhotoSearchMove
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(474, 251);
            Controls.Add(pbProcessFiles);
            Controls.Add(Btn_Cancel);
            Controls.Add(LblFileProc);
            Controls.Add(LblNumFiles);
            Controls.Add(Btn_Start);
            Controls.Add(Btn_DirDest);
            Controls.Add(Lbl_DirDestination);
            Controls.Add(Btn_DirSearch);
            Controls.Add(Lbl_DirSearch);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "FrmPhotoSearchMove";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Photo/Video Search, Copy And Divide to Year - Month";
            FormClosing += FrmPhotoSearchMove_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

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
    }
}
