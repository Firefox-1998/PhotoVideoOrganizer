using System.Collections.Concurrent;

namespace PhotoMoveYearMonthFolder
{
    public partial class FrmPhotoSearchMove : Form
    {
        private const string SuffixLogFile = "yyyyMMdd-HHmmss";
        private string sSearchDir = "";
        private string sDestDir = "";
        private bool isProcessing;
        private CancellationTokenSource? _cancellationTokenSource;
        private int processedFiles = 0;
        private int processedOtherFiles = 0;
        private ConcurrentDictionary<(string name, long size), byte> processedFileKeys = new();
        private readonly Dictionary<string, int> prefixToIndexMap = new()
        {
            {"IMG-", 4},
            {"IMG_", 4},
            {"VID-", 4},
            {"AUD-", 4},
            {"PPT-", 4},
            {"Screenshot_", 11},
            {"VideoCapture_", 13},
            {"IMG", 3},
            {"VID", 3},
            {"WP_", 3},
        };

        public FrmPhotoSearchMove()
        {
            InitializeComponent();

            // Sottoscrivi evento globale per aggiornare i testi quando cambia la cultura
            LocalizationManager.CultureChanged += LocalizationManager_CultureChanged;

            // Imposta lingua di default (English US)
            LocalizationManager.SetCultureByIndex(0);

            // Seleziona voce nella combo (triggererà SelectedIndexChanged ma SetCulture è idempotente)
            if (cmbLanguage.Items.Count > 0)
            {
                cmbLanguage.SelectedIndex = 0;
            }
        }

        private void LocalizationManager_CultureChanged(object? sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke((Action)UpdateTextsFromResources);
            }
            else
            {
                UpdateTextsFromResources();
            }
        }

        private void UpdateTextsFromResources()
        {
            // Riassegna i testi visibili dai resource properties (valutati con CurrentUICulture)
            Btn_DirDest.Text = PhotoSearchMove.FrmPhotoSearchMove_SelectDirectoryToImageCopy;
            Lbl_DirSearch.Text = string.IsNullOrEmpty(sSearchDir) 
                ? PhotoSearchMove.FrmPhotoSearchMove_DirectorySearch 
                : sSearchDir;
            Btn_DirSearch.Text = PhotoSearchMove.FrmPhotoSearchMove_SelectDirectoryToImageSearch;
            Btn_Cancel.Text = PhotoSearchMove.FrmPhotoSearchMove_Cancel;
            Btn_Start.Text = PhotoSearchMove.FrmPhotoSearchMove_Start;
            Lbl_DirDestination.Text = string.IsNullOrEmpty(sDestDir) 
                ? PhotoSearchMove.FrmPhotoSearchMove_DirectoryDestination 
                : sDestDir;
            lblMaxThread.Text = PhotoSearchMove.TbMaxThread_Scroll_MaxThread + tbMaxThread.Value.ToString();
            Btn_Exit.Text = PhotoSearchMove.FrmPhotoSearchMove_Exit;
            lblComment.Text = PhotoSearchMove.FrmPhotoSearchMove_lblComment;
            chkRootOnly.Text = PhotoSearchMove.FrmPhotoSearchMove_SearchImageSelectedRootOnly;
        }

        private void CmbLanguage_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cmbLanguage.SelectedIndex >= 0)
            {
                LocalizationManager.SetCultureByIndex(cmbLanguage.SelectedIndex);
            }
        }

        private async void Btn_Start_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(sSearchDir) && !string.IsNullOrEmpty(sDestDir) && !sSearchDir.Equals(sDestDir))
            {
                _cancellationTokenSource = new();
                Logger.SetLogFilePath(Path.Combine(sDestDir, $"{DateTime.Now.ToString(SuffixLogFile)}_PhotoSearchCopyAppLog.txt"));
                Logger.SetErrorFilePath(Path.Combine(sDestDir, $"{DateTime.Now.ToString(SuffixLogFile)}_PhotoSearchCopyErrLog.txt"));
                tbMaxThread.Enabled = false;
                Btn_DirDest.Enabled = false;
                Btn_DirSearch.Enabled = false;
                Btn_Start.Enabled = false;
                Btn_Cancel.Enabled = true;
                Btn_Exit.Enabled = false;
                chkRootOnly.Enabled = false;
                cmbLanguage.Enabled = false;
                isProcessing = true;
                processedFiles = 0;
                processedOtherFiles = 0;

                try
                {
                    processedFileKeys = new();
                    (IEnumerable<string> validFiles, IEnumerable<string> invalidFiles) = GetFiles(sSearchDir, chkRootOnly.Checked);

                    ParallelOptions parallelOptions = new()
                    {
                        MaxDegreeOfParallelism = tbMaxThread.Value,
                        CancellationToken = _cancellationTokenSource.Token
                    };

                    // Processo i file validi
                    LblNumFiles.Text = string.Format(PhotoSearchMove.Btn_Start_Click_NumFileProcessed, "0");
                    pbProcessFiles.Style = ProgressBarStyle.Marquee;
                    Logger.Log(Logging.Btn_Start_Click_STARTVALIDMEDIA);
                    await Parallel.ForEachAsync(validFiles, parallelOptions, async (file, token) =>
                    {
                        await ProcessFileAsync(file, LblFileProc, LblNumFiles);
                    });
                    Logger.Log(Logging.Btn_Start_Click_ENDVALIDMEDIA);
                    pbProcessFiles.Style = ProgressBarStyle.Blocks;
                    pbProcessFiles.Value = pbProcessFiles.Maximum;

                    // Processo gli altri file
                    processedFileKeys = new();
                    LblNumOtherFiles.Text = string.Format(PhotoSearchMove.Btn_Start_Click_NumOtherFileProcessed, "0");
                    pbProcessedOtherFiles.Style = ProgressBarStyle.Marquee;
                    Logger.Log(Logging.Btn_Start_Click_STARTNOTVALIDMEDIA);
                    await Parallel.ForEachAsync(invalidFiles, parallelOptions, async (file, token) =>
                    {
                        await ProcessFileAsyncNotValidExt(file, LblOtherFileProc, LblNumOtherFiles);
                    });
                    Logger.Log(Logging.Btn_Start_Click_ENDNOTVALIDMEDIA);
                    pbProcessedOtherFiles.Style = ProgressBarStyle.Blocks;
                    pbProcessedOtherFiles.Value = pbProcessedOtherFiles.Maximum;

                    MessageBox.Show(PhotoSearchMove.Btn_Start_Click_ElaborazioneCompletata,
                                    PhotoSearchMove.Btn_Start_Click_Informazioni,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                }
                catch (OperationCanceledException)
                {
                    MessageBox.Show(PhotoSearchMove.Btn_Start_Click_ElaborazioneAnnullataDallUtente,
                                    PhotoSearchMove.Btn_Start_Click_Annullato,
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    if (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        Logger.LogError(string.Format(Logging.Btn_Start_Click_ERRORExMessage, ex.Message));
                        MessageBox.Show(string.Format(Logging.Btn_Start_Click_ERRORExMessage, ex.Message),
                                                      Logging.Btn_Start_Click_Error,
                                                      MessageBoxButtons.OK,
                                                      MessageBoxIcon.Error);
                    }
                }
                finally
                {
                    _cancellationTokenSource?.Dispose();
                    LblNumFiles.Text = "-";
                    LblFileProc.Text = "-";
                    LblNumOtherFiles.Text = "-";
                    LblOtherFileProc.Text = "-";
                    pbProcessFiles.Style = ProgressBarStyle.Blocks;
                    pbProcessFiles.Value = 0;
                    pbProcessedOtherFiles.Style = ProgressBarStyle.Blocks;
                    pbProcessedOtherFiles.Value = 0;
                    tbMaxThread.Enabled = true;
                    Btn_DirDest.Enabled = true;
                    Btn_DirSearch.Enabled = true;
                    Btn_Start.Enabled = true;
                    Btn_Exit.Enabled = true;
                    chkRootOnly.Enabled = true;
                    cmbLanguage.Enabled = true;
                    if (Btn_Cancel.Enabled)
                    {
                        Btn_Cancel.Enabled = false;
                    }
                    else
                    {
                        Btn_Cancel.Text = PhotoSearchMove.FrmPhotoSearchMove_Cancel;
                    }
                    isProcessing = false;
                    Logger.FlushNow();
                }
            }
            else
            {
                MessageBox.Show(PhotoSearchMove.Btn_Start_Click_PleaseVerifyDirectoriesAreCorrect,
                                PhotoSearchMove.FrmPhotoSearchMove_FormClosing_Warning,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }
        }

        private void Btn_DirSearch_Click(object sender, EventArgs e)
        {
            if (Fbd_DirSel.ShowDialog() == DialogResult.OK)
            {
                sSearchDir = Fbd_DirSel.SelectedPath;
                Lbl_DirSearch.Text = sSearchDir;
            }
        }

        private void Btn_DirDest_Click(object sender, EventArgs e)
        {
            if (Fbd_DirSel.ShowDialog() == DialogResult.OK)
            {
                sDestDir = Fbd_DirSel.SelectedPath;
                Lbl_DirDestination.Text = sDestDir;
            }
        }

        private async Task ProcessFileAsync(string file, Label lblFileProc, Label lblNumFiles)
        {
            try
            {
                FileInfo fileInfo = new(file);
                (string Name, long Length) fileKey = (fileInfo.Name, fileInfo.Length);
                if (!processedFileKeys.TryAdd(fileKey, 0))
                {
                    Logger.Log(string.Format(Logging.ProcessFileAsync_SkippedNameAndSizeAlreadySeen, file));
                    return;
                }

                (string anno, string mese) = RecuperaMeseAnnoDaNomeFile(Path.GetFileNameWithoutExtension(file), file);

                string cartellaAnno = Path.Combine(sDestDir, anno);
                Directory.CreateDirectory(cartellaAnno);

                string cartellaMese = Path.Combine(cartellaAnno, mese);
                Directory.CreateDirectory(cartellaMese);

                string destinazioneFile = Path.Combine(cartellaMese, fileInfo.Name);

                if (System.IO.File.Exists(destinazioneFile))
                {
                    FileInfo destInfo = new(destinazioneFile);
                    if (fileInfo.Length == destInfo.Length)
                    {
                        string sourceHash = FrmPhotoSearchMoveHelpers.ComputeHash(file);
                        string destHash = FrmPhotoSearchMoveHelpers.ComputeHash(destinazioneFile);
                        if (string.Equals(sourceHash, destHash, StringComparison.Ordinal))
                        {
                            Logger.Log(string.Format(Logging.ProcessFileAsync_SkippedDuplicateConfirmedByHash, file, destinazioneFile));
                            return;
                        }
                    }

                    if (FrmPhotoSearchMoveHelpers.IsImageFileFast(file))
                    {
                        string srcId = FrmPhotoSearchMoveHelpers.ReadExifUniqueImageID(file);
                        if (!string.IsNullOrEmpty(srcId))
                        {
                            string dstId = FrmPhotoSearchMoveHelpers.ReadExifUniqueImageID(destinazioneFile);
                            if (srcId == dstId)
                            {
                                Logger.Log(string.Format(Logging.ProcessFileAsync_SkippedSameEXIFUniqueImageID, file, destinazioneFile));
                                return;
                            }
                        }
                    }

                    string newDestFile = FrmPhotoSearchMoveHelpers.GenerateNewFileName(destinazioneFile);
                    string written = await FrmPhotoSearchMoveHelpers.CopyFileWithUniqueNameAsync(file, newDestFile);
                    Logger.Log(string.Format(Logging.ProcessFileAsyncValidExt_Copied, file, written));
                }
                else
                {
                    string written = await FrmPhotoSearchMoveHelpers.CopyFileWithUniqueNameAsync(file, destinazioneFile);
                    Logger.Log(string.Format(Logging.ProcessFileAsyncValidExt_Copied, file, written));
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Logger.LogError(string.Format(Logging.ProcessFileAsync_ErrorDuringProcessingOf, file, ex.Message));
            }
            finally
            {
                int count = Interlocked.Increment(ref processedFiles);
                lblFileProc.Invoke((Action)(() => lblFileProc.Text = Path.GetFileName(file)));
                lblNumFiles.Invoke((Action)(() => lblNumFiles.Text = string.Format(PhotoSearchMove.Btn_Start_Click_NumFileProcessed, count)));
            }
        }

        private async Task ProcessFileAsyncNotValidExt(string file, Label lblOtherFileProc, Label lblNumOtherFiles)
        {
            try
            {
                FileInfo fileInfo = new(file);
                (string Name, long Length) fileKey = (fileInfo.Name, fileInfo.Length);
                if (!processedFileKeys.TryAdd(fileKey, 0))
                {
                    Logger.Log(string.Format(Logging.ProcessFileAsync_SkippedNameAndSizeAlreadySeen, file));
                    return;
                }

                string cartellaOtherExt = Path.Combine(sDestDir, "OtherFilesExt");
                Directory.CreateDirectory(cartellaOtherExt);

                string destinazioneFile = Path.Combine(cartellaOtherExt, fileInfo.Name);

                if (System.IO.File.Exists(destinazioneFile))
                {
                    FileInfo destInfo = new(destinazioneFile);
                    if (fileInfo.Length == destInfo.Length)
                    {
                        string sourceHash = FrmPhotoSearchMoveHelpers.ComputeHash(file);
                        string destHash = FrmPhotoSearchMoveHelpers.ComputeHash(destinazioneFile);
                        if (string.Equals(sourceHash, destHash, StringComparison.Ordinal))
                        {
                            Logger.Log(string.Format(Logging.ProcessFileAsync_SkippedDuplicateConfirmedByHash, file, destinazioneFile));
                            return;
                        }
                    }

                    string newDestFile = FrmPhotoSearchMoveHelpers.GenerateNewFileName(destinazioneFile);
                    string written = await FrmPhotoSearchMoveHelpers.CopyFileWithUniqueNameAsync(file, newDestFile);
                    Logger.Log(string.Format(Logging.ProcessFileAsyncNotValidExt_Copied, file, written));
                }
                else
                {
                    string written = await FrmPhotoSearchMoveHelpers.CopyFileWithUniqueNameAsync(file, destinazioneFile);
                    Logger.Log(string.Format(Logging.ProcessFileAsyncNotValidExt_Copied, file, written));
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Logger.LogError(string.Format(Logging.ProcessFileAsync_ErrorDuringProcessingOf, file, ex.Message));
            }
            finally
            {
                int count = Interlocked.Increment(ref processedOtherFiles);
                lblOtherFileProc.Invoke((Action)(() => lblOtherFileProc.Text = Path.GetFileName(file)));
                lblNumOtherFiles.Invoke((Action)(() => lblNumOtherFiles.Text = string.Format(
                    PhotoSearchMove.Btn_Start_Click_NumOtherFileProcessed,
                    count)));
            }
        }

        private void FrmPhotoSearchMove_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isProcessing)
            {
                e.Cancel = true;
                MessageBox.Show(PhotoSearchMove.FrmPhotoSearchMove_FormClosing_CannotBeClosedWhileProcessingIsInProgress,
                                PhotoSearchMove.FrmPhotoSearchMove_FormClosing_Warning,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }
            else
            {
                Logger.Log(Logging.FrmPhotoSearchMove_FormClosing_EXIT);
                Logger.LogError(Logging.FrmPhotoSearchMove_FormClosing_EXIT);
                Logger.FlushNow();
                Logger.Shutdown();
            }
        }

        private void Btn_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult Cancelrequest = MessageBox.Show(PhotoSearchMove.Btn_Cancel_Click_StopProcessing,
                                                         PhotoSearchMove.Btn_Start_Click_Informazioni,
                                                         MessageBoxButtons.YesNo,
                                                         MessageBoxIcon.Information);
            if (Cancelrequest == DialogResult.Yes)
            {
                _cancellationTokenSource?.Cancel();
                Logger.Log(Logging.Btn_Cancel_Click_CANCELREQUEST);
                Logger.LogError(Logging.Btn_Cancel_Click_CANCELREQUEST);

                Btn_Cancel.Enabled = false;
                Btn_Cancel.Text = PhotoSearchMove.Btn_Cancel_Click_CANCELREQUESTRNWait;
            }
        }

        private void TbMaxThread_Scroll(object sender, EventArgs e)
        {
            lblMaxThread.Text = PhotoSearchMove.TbMaxThread_Scroll_MaxThread + tbMaxThread.Value.ToString();
        }

        public static (IEnumerable<string> validFiles, IEnumerable<string> invalidFiles) GetFiles(string rootPath, bool rootOnly)
        {
            EnumerationOptions enumerationOptions = new()
            {
                RecurseSubdirectories = !rootOnly,
                IgnoreInaccessible = true,
                AttributesToSkip = FileAttributes.System | FileAttributes.Hidden
            };

            IEnumerable<string> allFiles;
            try
            {
                DirectoryInfo root = new(rootPath);
                allFiles = (root.Attributes & (FileAttributes.System | FileAttributes.Hidden)) != 0
                    ? Enumerable.Empty<string>()
                    : Directory.EnumerateFiles(rootPath, "*", enumerationOptions);
            }
            catch
            {
                allFiles = Enumerable.Empty<string>();
            }

            HashSet<string> excludedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".ini", ".db", ".com", ".exe", ".dll", ".txt" };

            ILookup<bool, string> partitionedFiles = allFiles
                .Where(file => !excludedExtensions.Contains(Path.GetExtension(file)))
                .ToLookup(FrmPhotoSearchMoveHelpers.IsValidMediaBySniff);

            return (
                validFiles: partitionedFiles[true],
                invalidFiles: partitionedFiles[false]
            );
        }

        public (string, string) RecuperaMeseAnnoDaNomeFile(string nomeFile, string file)
        {
            string anno = "1970";
            string mese = "01";
            bool matchFound = false;

            foreach (KeyValuePair<string, int> entry in prefixToIndexMap)
            {
                if (!nomeFile.StartsWith(entry.Key, StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    string possibleAnno = nomeFile[entry.Value..(entry.Value + 4)];
                    string possibleMese = nomeFile[(entry.Value + 4)..(entry.Value + 6)];
                    int year = int.Parse(possibleAnno);

                    if (year >= 1970 && year <= DateTime.Now.Year)
                    {
                        anno = possibleAnno;
                        mese = possibleMese;
                        matchFound = true;
                        break;
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    // continua
                }
            }

            if (!matchFound)
            {
                (anno, mese) = ClsDateExtractor.ExtractYearMonth(nomeFile);
                int parsedYear = int.Parse(anno);

                if (parsedYear < 1970 || parsedYear > DateTime.Now.Year)
                {
                    string parsedDate = FrmPhotoSearchMoveHelpers.ReadExifDataOrFileSystemDate(file);
                    anno = parsedDate[..4];
                    mese = parsedDate[4..6];
                }
            }

            return (anno, mese);
        }

        private void Btn_Exit_Click(object sender, EventArgs e)
        {
            Logger.Log(Logging.FrmPhotoSearchMove_FormClosing_EXIT);
            Logger.LogError(Logging.FrmPhotoSearchMove_FormClosing_EXIT);
            Logger.FlushNow();
            Logger.Shutdown();
            Application.Exit();
        }

        private void LblComment_Click(object sender, EventArgs e)
        {
            if (!isProcessing)
            {
                using FrmAbout about = new();
                about.ShowDialog();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                LocalizationManager.CultureChanged -= LocalizationManager_CultureChanged;
                components?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}