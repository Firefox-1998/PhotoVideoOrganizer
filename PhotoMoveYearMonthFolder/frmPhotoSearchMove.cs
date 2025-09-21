using System.Collections.Concurrent;
using System.Threading.Tasks;

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
                    LblNumFiles.Text = "Num. file processati: 0";
                    pbProcessFiles.Style = ProgressBarStyle.Marquee;
                    Logger.Log(">>> START VALID MEDIA <<<");
                    await Parallel.ForEachAsync(validFiles, parallelOptions, async (file, token) =>
                    {
                        await ProcessFileAsync(file, LblFileProc, LblNumFiles);
                    });
                    Logger.Log(">>> END VALID MEDIA <<<");
                    pbProcessFiles.Style = ProgressBarStyle.Blocks;
                    pbProcessFiles.Value = pbProcessFiles.Maximum; // Mostra completato

                    // Processo gli altri file
                    processedFileKeys = new();
                    LblNumOtherFiles.Text = "Num. altri file processati: 0";
                    pbProcessedOtherFiles.Style = ProgressBarStyle.Marquee;
                    Logger.Log("\r\n---------------------------------\r\n>>> START >> NOT << VALID MEDIA <<<");
                    await Parallel.ForEachAsync(invalidFiles, parallelOptions, async (file, token) =>
                    {
                        await ProcessFileAsyncNotValidExt(file, LblOtherFileProc, LblNumOtherFiles);
                    });
                    Logger.Log(">>> END > NOT < VALID MEDIA <<<");
                    pbProcessedOtherFiles.Style = ProgressBarStyle.Blocks;
                    pbProcessedOtherFiles.Value = pbProcessedOtherFiles.Maximum; // Mostra completato

                    MessageBox.Show("Elaborazione completata!", "Informazioni", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (OperationCanceledException)
                {
                    MessageBox.Show("Elaborazione annullata dall'utente.", "Annullato", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    if (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        Logger.LogError($">>> ERRORE: {ex.Message} <<<");
                        MessageBox.Show($"Si è verificato un errore: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    if (Btn_Cancel.Enabled)
                    {
                        Btn_Cancel.Enabled = false;
                    }
                    else
                    {
                        Btn_Cancel.Text = "Cancel";
                    }
                    isProcessing = false;
                    Logger.FlushNow();
                }
            }
            else
            {
                MessageBox.Show($"Verificare la corretezza delle directory di origine e destinazione.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    Logger.Log($"Saltato (nome e dimensione già visti) {file}");
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
                        // Solo ora, in caso di collisione, calcoliamo gli hash
                        string sourceHash = FrmPhotoSearchMoveHelpers.ComputeHash(file);
                        string destHash = FrmPhotoSearchMoveHelpers.ComputeHash(destinazioneFile);
                        if (string.Equals(sourceHash, destHash, StringComparison.Ordinal))
                        {
                            Logger.Log($"Saltato (duplicato confermato da hash) {file} -> {destinazioneFile}");
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
                                Logger.Log($"Saltato (stesso EXIF UniqueImageID) {file} -> {destinazioneFile}");
                                return;
                            }
                        }
                    }

                    string newDestFile = FrmPhotoSearchMoveHelpers.GenerateNewFileName(destinazioneFile);
                    string written = await FrmPhotoSearchMoveHelpers.CopyFileWithUniqueNameAsync(file, newDestFile);
                    Logger.Log($"Copiato {file} -> {written}");
                }
                else
                {
                    string written = await FrmPhotoSearchMoveHelpers.CopyFileWithUniqueNameAsync(file, destinazioneFile);
                    Logger.Log($"Copiato {file} -> {written}");
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Logger.LogError($"Errore durante l'elaborazione di {file}: {ex.Message}");
            }
            finally
            {
                int count = Interlocked.Increment(ref processedFiles);
                lblFileProc.Invoke((Action)(() => lblFileProc.Text = Path.GetFileName(file)));
                lblNumFiles.Invoke((Action)(() => lblNumFiles.Text = $"Num. file processati: {count}"));
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
                    Logger.Log($"Saltato (nome e dimensione già visti) {file}");
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
                            Logger.Log($"Saltato (duplicato confermato da hash) {file} -> {destinazioneFile}");
                            return;
                        }
                    }

                    string newDestFile = FrmPhotoSearchMoveHelpers.GenerateNewFileName(destinazioneFile);
                    string written = await FrmPhotoSearchMoveHelpers.CopyFileWithUniqueNameAsync(file, newDestFile);
                    Logger.Log($"Copiato {file} -> {written}");
                }
                else
                {
                    string written = await FrmPhotoSearchMoveHelpers.CopyFileWithUniqueNameAsync(file, destinazioneFile);
                    Logger.Log($"Copiato {file} -> {written}");
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Logger.LogError($"Errore durante l'elaborazione di {file}: {ex.Message}");
            }
            finally
            {
                int count = Interlocked.Increment(ref processedOtherFiles);
                lblOtherFileProc.Invoke((Action)(() => lblOtherFileProc.Text = Path.GetFileName(file)));
                lblNumOtherFiles.Invoke((Action)(() => lblNumOtherFiles.Text = $"Num. altri file processati: {count}"));
            }
        }

        private void FrmPhotoSearchMove_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isProcessing)
            {
                e.Cancel = true;
                MessageBox.Show("Non è possibile chiudere la form durante l'elaborazione.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                Logger.Log($">>> EXIT <<<");
                Logger.LogError($">>> EXIT <<<");
                Logger.FlushNow();
                Logger.Shutdown();
            }
        }

        private void Btn_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult Cancelrequest = MessageBox.Show("Confermi l'interruzione dell'elaborazione?", "Info", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (Cancelrequest == DialogResult.Yes)
            {
                _cancellationTokenSource?.Cancel();
                Logger.Log($">>> CANCEL REQUEST !!! <<<");
                Logger.LogError($">>> CANCEL REQUEST !!! <<<");

                Btn_Cancel.Enabled = false;
                Btn_Cancel.Text = "CANCEL REQUEST\r\nWait...";
            }
        }

        private void TbMaxThread_Scroll(object sender, EventArgs e)
        {
            lblMaxThread.Text = "Max Thread: " + tbMaxThread.Value.ToString();
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
                    ? []
                    : Directory.EnumerateFiles(rootPath, "*", enumerationOptions);
            }
            catch
            {
                allFiles = [];
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
            Logger.Log($">>> EXIT <<<");
            Logger.LogError($">>> EXIT <<<");
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
    }
}