using PhotoMoveYearMonthFolder.Resources;
using PhotoMoveYearMonthFolder.Services;

namespace PhotoMoveYearMonthFolder
{
    /// <summary>
    /// Main form for the Photo/Video Organizer application.
    /// Acts as a thin controller coordinating services and UI.
    /// </summary>
    public partial class FrmPhotoSearchMove : Form, IProgressReporter
    {
        #region Constants

        private const string SuffixLogFile = "yyyyMMdd-HHmmss";
        private const string LogFileSuffix = "_PhotoSearchCopyAppLog.txt";
        private const string ErrorLogFileSuffix = "_PhotoSearchCopyErrLog.txt";

        #endregion

        #region Fields

        private string _searchDir = string.Empty;
        private string _destDir = string.Empty;
        private bool _isProcessing;
        private CancellationTokenSource? _cancellationTokenSource;
        private FileProcessingService? _processingService;

        #endregion

        #region Constructor

        public FrmPhotoSearchMove()
        {
            InitializeComponent();
            InitializeLocalization();
        }

        private void InitializeLocalization()
        {
            LocalizationManager.CultureChanged += OnCultureChanged;
            LocalizationManager.SetCultureByIndex(0);

            if (cmbLanguage.Items.Count > 0)
            {
                cmbLanguage.SelectedIndex = 0;
            }
        }

        #endregion

        #region IProgressReporter Implementation

        void IProgressReporter.ReportMediaFileProgress(int count, string fileName)
        {
            SafeUpdateLabel(LblFileProc, Path.GetFileName(fileName));
            SafeUpdateLabel(LblNumFiles, string.Format(PhotoSearchMove.Btn_Start_Click_NumFileProcessed, count));
        }

        void IProgressReporter.ReportOtherFileProgress(int count, string fileName)
        {
            SafeUpdateLabel(LblOtherFileProc, Path.GetFileName(fileName));
            SafeUpdateLabel(LblNumOtherFiles, string.Format(PhotoSearchMove.Btn_Start_Click_NumOtherFileProcessed, count));
        }

        void IProgressReporter.SetMediaProgressMarquee()
        {
            SafeUpdateLabel(LblNumFiles, string.Format(PhotoSearchMove.Btn_Start_Click_NumFileProcessed, "0"));
            SafeUpdateProgressBar(pbProcessFiles, style: ProgressBarStyle.Marquee);
        }

        void IProgressReporter.SetOtherProgressMarquee()
        {
            SafeUpdateLabel(LblNumOtherFiles, string.Format(PhotoSearchMove.Btn_Start_Click_NumOtherFileProcessed, "0"));
            SafeUpdateProgressBar(pbProcessedOtherFiles, style: ProgressBarStyle.Marquee);
        }

        void IProgressReporter.CompleteMediaProgress()
        {
            SafeUpdateProgressBar(pbProcessFiles, style: ProgressBarStyle.Blocks, value: pbProcessFiles.Maximum);
        }

        void IProgressReporter.CompleteOtherProgress()
        {
            SafeUpdateProgressBar(pbProcessedOtherFiles, style: ProgressBarStyle.Blocks, value: pbProcessedOtherFiles.Maximum);
        }

        void IProgressReporter.ResetProgress()
        {
            // Only reset progress labels and bars, NOT the Cancel button state
            ResetProgressLabelsAndBars();
        }

        void IProgressReporter.ReportEnumerationProgress(int totalFound, int classifiedAsMedia, int classifiedAsOther)
        {
            SafeUpdateLabel(LblNumFiles,
                string.Format(PhotoSearchMove.Btn_Start_Click_EnumeratingFiles,
                    totalFound, classifiedAsMedia, classifiedAsOther));
            SafeUpdateLabel(LblFileProc, "⏳");
        }

        #endregion

        #region Localization

        private void OnCultureChanged(object? sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(UpdateTextsFromResources);
            }
            else
            {
                UpdateTextsFromResources();
            }
        }

        private void UpdateTextsFromResources()
        {
            lblLanguageUI.Text = PhotoSearchMove.FrmPhotoSearchMove_lblLanguageUI;
            Btn_DirDest.Text = PhotoSearchMove.FrmPhotoSearchMove_SelectDirectoryToImageCopy;
            Btn_DirSearch.Text = PhotoSearchMove.FrmPhotoSearchMove_SelectDirectoryToImageSearch;
            Btn_Cancel.Text = PhotoSearchMove.FrmPhotoSearchMove_Cancel;
            Btn_Start.Text = PhotoSearchMove.FrmPhotoSearchMove_Start;
            Btn_Exit.Text = PhotoSearchMove.FrmPhotoSearchMove_Exit;
            lblComment.Text = PhotoSearchMove.FrmPhotoSearchMove_lblComment;
            chkRootOnly.Text = PhotoSearchMove.FrmPhotoSearchMove_SearchImageSelectedRootOnly;
            lblMaxThread.Text = PhotoSearchMove.TbMaxThread_Scroll_MaxThread + tbMaxThread.Value.ToString();

            Lbl_DirSearch.Text = string.IsNullOrEmpty(_searchDir)
                ? PhotoSearchMove.FrmPhotoSearchMove_DirectorySearch
                : _searchDir;

            Lbl_DirDestination.Text = string.IsNullOrEmpty(_destDir)
                ? PhotoSearchMove.FrmPhotoSearchMove_DirectoryDestination
                : _destDir;
        }

        private void CmbLanguage_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbLanguage.SelectedIndex >= 0)
            {
                LocalizationManager.SetCultureByIndex(cmbLanguage.SelectedIndex);
            }
        }

        #endregion

        #region UI State Management

        private void SetUIProcessingState(bool processing)
        {
            _isProcessing = processing;

            Control[] interactiveControls = [tbMaxThread, Btn_DirDest, Btn_DirSearch, Btn_Start, Btn_Exit, chkRootOnly, cmbLanguage];
            foreach (Control control in interactiveControls)
            {
                control.Enabled = !processing;
            }

            // Cancel button is enabled only during processing
            Btn_Cancel.Enabled = processing;
            Btn_Cancel.Text = PhotoSearchMove.FrmPhotoSearchMove_Cancel;

            if (!processing)
            {
                ResetProgressLabelsAndBars();
            }
        }

        /// <summary>
        /// Resets only the progress labels and progress bars.
        /// Does NOT affect the Cancel button state.
        /// </summary>
        private void ResetProgressLabelsAndBars()
        {
            SafeUpdateLabel(LblNumFiles, "-");
            SafeUpdateLabel(LblFileProc, "-");
            SafeUpdateLabel(LblNumOtherFiles, "-");
            SafeUpdateLabel(LblOtherFileProc, "-");

            SafeUpdateProgressBar(pbProcessFiles, style: ProgressBarStyle.Blocks, value: 0);
            SafeUpdateProgressBar(pbProcessedOtherFiles, style: ProgressBarStyle.Blocks, value: 0);
        }

        #endregion

        #region Thread-Safe UI Updates

        private void SafeUpdateLabel(Label label, string text)
        {
            if (IsDisposed) return;

            try
            {
                if (label.InvokeRequired)
                {
                    BeginInvoke(() =>
                    {
                        if (!IsDisposed && !label.IsDisposed)
                            label.Text = text;
                    });
                }
                else if (!label.IsDisposed)
                {
                    label.Text = text;
                }
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }

        private void SafeUpdateProgressBar(ProgressBar progressBar, ProgressBarStyle? style = null, int? value = null)
        {
            if (IsDisposed) return;

            try
            {
                Action updateAction = () =>
                {
                    if (IsDisposed || progressBar.IsDisposed) return;

                    if (style.HasValue)
                        progressBar.Style = style.Value;

                    if (value.HasValue)
                        progressBar.Value = value.Value;
                };

                if (progressBar.InvokeRequired)
                {
                    BeginInvoke(updateAction);
                }
                else
                {
                    updateAction();
                }
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }

        #endregion

        #region Directory Selection

        private void Btn_DirSearch_Click(object sender, EventArgs e)
        {
            if (Fbd_DirSel.ShowDialog() == DialogResult.OK)
            {
                _searchDir = Fbd_DirSel.SelectedPath;
                Lbl_DirSearch.Text = _searchDir;
            }
        }

        private void Btn_DirDest_Click(object sender, EventArgs e)
        {
            if (Fbd_DirSel.ShowDialog() == DialogResult.OK)
            {
                _destDir = Fbd_DirSel.SelectedPath;
                Lbl_DirDestination.Text = _destDir;
            }
        }

        private bool ValidateDirectories()
        {
            if (string.IsNullOrEmpty(_searchDir) ||
                string.IsNullOrEmpty(_destDir) ||
                _searchDir.Equals(_destDir, StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    PhotoSearchMove.Btn_Start_Click_PleaseVerifyDirectoriesAreCorrect,
                    PhotoSearchMove.FrmPhotoSearchMove_FormClosing_Warning,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        #endregion

        #region Main Processing

        private async void Btn_Start_Click(object sender, EventArgs e)
        {
            if (!ValidateDirectories()) return;

            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = _cancellationTokenSource.Token;

            int maxThreads = tbMaxThread.Value;
            bool rootOnly = chkRootOnly.Checked;
            string searchDir = _searchDir;
            string destDir = _destDir;

            InitializeLogging(destDir);
            SetUIProcessingState(true);

            _processingService = new FileProcessingService(this);

            try
            {
                await _processingService.ProcessAllFilesAsync(
                    searchDir,
                    destDir,
                    rootOnly,
                    maxThreads,
                    token);

                ShowCompletionMessage();
            }
            catch (OperationCanceledException)
            {
                ShowCancellationMessage();
            }
            catch (Exception ex) when (!token.IsCancellationRequested)
            {
                HandleProcessingError(ex);
            }
            finally
            {
                SetUIProcessingState(false);
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                _processingService = null;
            }
        }

        private static void InitializeLogging(string destDir)
        {
            string timestamp = DateTime.Now.ToString(SuffixLogFile);
            Logger.SetLogFilePath(Path.Combine(destDir, timestamp + LogFileSuffix));
            Logger.SetErrorFilePath(Path.Combine(destDir, timestamp + ErrorLogFileSuffix));
        }

        #endregion

        #region User Messages

        private static void ShowCompletionMessage()
        {
            MessageBox.Show(
                PhotoSearchMove.Btn_Start_Click_ElaborazioneCompletata,
                PhotoSearchMove.Btn_Start_Click_Informazioni,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private static void ShowCancellationMessage()
        {
            MessageBox.Show(
                PhotoSearchMove.Btn_Start_Click_ElaborazioneAnnullataDallUtente,
                PhotoSearchMove.Btn_Start_Click_Annullato,
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        private static void HandleProcessingError(Exception ex)
        {
            Logger.LogError(string.Format(Logging.Btn_Start_Click_ERRORExMessage, ex.Message));
            MessageBox.Show(
                string.Format(Logging.Btn_Start_Click_ERRORExMessage, ex.Message),
                Logging.Btn_Start_Click_Error,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        #endregion

        #region Event Handlers

        private void TbMaxThread_Scroll(object sender, EventArgs e)
        {
            lblMaxThread.Text = PhotoSearchMove.TbMaxThread_Scroll_MaxThread + tbMaxThread.Value.ToString();
        }

        private void Btn_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                PhotoSearchMove.Btn_Cancel_Click_StopProcessing,
                PhotoSearchMove.Btn_Start_Click_Informazioni,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
                _cancellationTokenSource?.Cancel();
                Logger.Log(Logging.Btn_Cancel_Click_CANCELREQUEST);
                Logger.LogError(Logging.Btn_Cancel_Click_CANCELREQUEST);

                Btn_Cancel.Enabled = false;
                Btn_Cancel.Text = PhotoSearchMove.Btn_Cancel_Click_CANCELREQUESTRNWait;
            }
        }

        private void Btn_Exit_Click(object sender, EventArgs e)
        {
            ShutdownApplication();
        }

        private void FrmPhotoSearchMove_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isProcessing)
            {
                e.Cancel = true;
                MessageBox.Show(
                    PhotoSearchMove.FrmPhotoSearchMove_FormClosing_CannotBeClosedWhileProcessingIsInProgress,
                    PhotoSearchMove.FrmPhotoSearchMove_FormClosing_Warning,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            else
            {
                ShutdownApplication();
            }
        }

        private void LblComment_Click(object sender, EventArgs e)
        {
            if (!_isProcessing)
            {
                using FrmAbout about = new();
                about.ShowDialog();
            }
        }

        private static void ShutdownApplication()
        {
            Logger.Log(Logging.FrmPhotoSearchMove_FormClosing_EXIT);
            Logger.LogError(Logging.FrmPhotoSearchMove_FormClosing_EXIT);
            Logger.Shutdown();
            Application.Exit();
        }

        #endregion

        #region IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                LocalizationManager.CultureChanged -= OnCultureChanged;
                _cancellationTokenSource?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}