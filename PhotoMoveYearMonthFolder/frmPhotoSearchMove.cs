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
        private ConcurrentDictionary<string, byte> fileHashes = new();
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
                    fileHashes = new();

                    // Processo i file validi (immagini, video, audio) rilevati via sniff
                    Logger.Log($">>> START VALID MEDIA <<<");
                    Logger.LogError($">>> START VALID MEDIA <<<");

                    var semaphore = new SemaphoreSlim(tbMaxThread.Value); // max concorrenza
                    var files = GetValidFiles(sSearchDir, chkRootOnly);
                    int numFiles = files.Count;

                    LblNumFiles.Text = $"Num. file da processare: {numFiles}";
                    pbProcessFiles.Maximum = numFiles;
                    pbProcessFiles.Value = 0;

                    var partitioner = Partitioner.Create(files, true);
                    var tasks = partitioner.GetPartitions(tbMaxThread.Value).Select(partition =>
                        Task.Run(async () =>
                        {
                            while (partition.MoveNext())
                            {
                                var file = partition.Current;
                                await semaphore.WaitAsync(_cancellationTokenSource.Token);
                                try
                                {
                                    await ProcessFileAsync(file, LblFileProc, pbProcessFiles);
                                }
                                finally
                                {
                                    semaphore.Release();
                                }
                            }
                        }, _cancellationTokenSource.Token));

                    await Task.WhenAll(tasks);
                    Logger.Log($">>> END VALID MEDIA <<<");
                    Logger.LogError($">>> END VALID MEDIA <<<");

                    // Processo gli altri file (non riconosciuti da sniff) nella cartella OtherFilesExt
                    files = GetInvalidFiles(sSearchDir, chkRootOnly);
                    int numOtherFiles = files.Count;
                    LblNumOtherFiles.Text = $"Num. altri file da processare: {numOtherFiles}";
                    if (numOtherFiles != 0)
                    {
                        fileHashes = new();
                        pbProcessedOtherFiles.Maximum = numOtherFiles;
                        pbProcessedOtherFiles.Value = 0;

                        Logger.Log($"\r\n---------------------------------\r\n");
                        Logger.LogError($"\r\n---------------------------------\r\n");
                        Logger.Log($">>> START >> NOT << VALID MEDIA <<<");
                        Logger.LogError($">>> START >> NOT << VALID MEDIA <<<");

                        tasks = files.Select(file =>
                            Task.Run(async () =>
                            {
                                await semaphore.WaitAsync(_cancellationTokenSource.Token);
                                try
                                {
                                    await ProcessFileAsyncNotValidExt(file, LblOtherFileProc, pbProcessedOtherFiles);
                                }
                                finally
                                {
                                    semaphore.Release();
                                }
                            }, _cancellationTokenSource.Token));

                        await Task.WhenAll(tasks);
                        Logger.Log($">>> END > NOT < VALID MEDIA <<<");
                        Logger.LogError($">>> END > NOT < VALID MEDIA <<<");
                    }

                    MessageBox.Show("Elaborazione completata!", "Informazioni", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    pbProcessFiles.Value = 0;
                    pbProcessFiles.Maximum = 100;
                    pbProcessedOtherFiles.Value = 0;
                    pbProcessedOtherFiles.Maximum = 100;
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

        private async Task ProcessFileAsync(string file, Label lblFileNumProc, ProgressBar pbNumFilesProc)
        {
            try
            {
                // Deduplica concorrente: se l'hash è già visto, esci subito (niente EXIF/I-O extra)
                string fileHash = FrmPhotoSearchMoveHelpers.ComputeHash(file);
                if (!fileHashes.TryAdd(fileHash, 0))
                {
                    Logger.Log($"Saltato (hash già visto) {file}");
                    return;
                }

                string nomeFile = Path.GetFileNameWithoutExtension(file);
                (string anno, string mese) = RecuperaMeseAnnoDaNomeFile(nomeFile, file);

                string cartellaAnno = Path.Combine(sDestDir, anno);
                Directory.CreateDirectory(cartellaAnno);

                string cartellaMese = Path.Combine(cartellaAnno, mese);
                Directory.CreateDirectory(cartellaMese);

                string destinazioneFile = Path.Combine(cartellaMese, nomeFile + Path.GetExtension(file));
                bool fileExists = File.Exists(destinazioneFile);

                if (fileExists)
                {
                    // Confronto hash del file di destinazione
                    string existingFileHash = FrmPhotoSearchMoveHelpers.ComputeHash(destinazioneFile);
                    if (string.Equals(fileHash, existingFileHash, StringComparison.Ordinal))
                    {
                        Logger.Log($"Saltato {file} {destinazioneFile}");
                        return;
                    }

                    // EXIF solo per immagini e solo in caso di collisione di nome con contenuti diversi
                    if (FrmPhotoSearchMoveHelpers.IsImageFileFast(file))
                    {
                        string srcId = FrmPhotoSearchMoveHelpers.ReadExifUniqueImageID(file);
                        string dstId = FrmPhotoSearchMoveHelpers.ReadExifUniqueImageID(destinazioneFile);
                        if (!string.IsNullOrEmpty(srcId) && !string.IsNullOrEmpty(dstId) && srcId == dstId)
                        {
                            Logger.Log($"Saltato. Stesso TAG EXIF 'ImageUniqueID' {file} {destinazioneFile}");
                            return;
                        }
                    }

                    // Copia con nome unico atomica
                    string destinationFile = FrmPhotoSearchMoveHelpers.GenerateNewFileName(destinazioneFile);
                    string written = await FrmPhotoSearchMoveHelpers.CopyFileWithUniqueNameAsync(file, destinationFile);
                    Logger.Log($"Copiato {file} {written}");
                }
                else
                {
                    // Copia con nome unico atomica anche qui per evitare race
                    string written = await FrmPhotoSearchMoveHelpers.CopyFileWithUniqueNameAsync(file, destinazioneFile);
                    Logger.Log($"Copiato {file} {written}");
                }
            }
            catch (FormatException)
            {
                Logger.LogError($"Eccezione formato data {file}");
            }
            finally
            {
                processedFiles = Interlocked.Increment(ref processedFiles);
                lblFileNumProc.Invoke((Action)(() => lblFileNumProc.Text = $"Num. file processati: {processedFiles}"));
                pbNumFilesProc.Invoke((Action)(() => pbNumFilesProc.Value = Math.Min(processedFiles, pbNumFilesProc.Maximum)));
            }
        }

        private async Task ProcessFileAsyncNotValidExt(string file, Label lblOtherFileNumProc, ProgressBar pbNumOtherFilesProc)
        {
            try
            {
                // Deduplica concorrente
                string fileHash = FrmPhotoSearchMoveHelpers.ComputeHash(file);
                if (!fileHashes.TryAdd(fileHash, 0))
                {
                    Logger.Log($"Saltato (hash già visto) {file}");
                    return;
                }

                string nomeFile = Path.GetFileNameWithoutExtension(file);
                string cartellaOtherExt = Path.Combine(sDestDir, "OtherFilesExt");
                Directory.CreateDirectory(cartellaOtherExt);

                string destinazioneFile = Path.Combine(cartellaOtherExt, nomeFile + Path.GetExtension(file));
                bool fileExists = File.Exists(destinazioneFile);

                if (fileExists)
                {
                    string existingFileHash = FrmPhotoSearchMoveHelpers.ComputeHash(destinazioneFile);
                    if (string.Equals(fileHash, existingFileHash, StringComparison.Ordinal))
                    {
                        Logger.Log($"Saltato {file} {destinazioneFile}");
                    }
                    else
                    {
                        string destinationFile = FrmPhotoSearchMoveHelpers.GenerateNewFileName(destinazioneFile);
                        string written = await FrmPhotoSearchMoveHelpers.CopyFileWithUniqueNameAsync(file, destinationFile);
                        Logger.Log($"Copiato {file} {written}");
                    }
                }
                else
                {
                    string written = await FrmPhotoSearchMoveHelpers.CopyFileWithUniqueNameAsync(file, destinazioneFile);
                    Logger.Log($"Copiato {file} {written}");
                }
            }
            catch (FormatException)
            {
                Logger.LogError($"Eccezione formato data {file}");
            }
            finally
            {
                processedOtherFiles = Interlocked.Increment(ref processedOtherFiles);
                lblOtherFileNumProc.Invoke((Action)(() => lblOtherFileNumProc.Text = $"Num. file processati: {processedOtherFiles}"));
                pbNumOtherFilesProc.Invoke((Action)(() => pbNumOtherFilesProc.Value = Math.Min(processedOtherFiles, pbNumOtherFilesProc.Maximum)));
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

        public static List<string> GetValidFiles(string rootPath, CheckBox cRootOnly)
        {
            var results = new List<string>();
            var root = new DirectoryInfo(rootPath);

            if ((root.Attributes & (FileAttributes.System | FileAttributes.Hidden)) != 0)
                return results;

            void Scan(string dir)
            {
                try
                {
                    foreach (var file in Directory.EnumerateFiles(dir))
                    {
                        try
                        {
                            if (FrmPhotoSearchMoveHelpers.IsValidMediaBySniff(file))
                                results.Add(file);
                        }
                        catch { }
                    }

                    if (!cRootOnly.Checked)
                    {
                        foreach (var sub in Directory.EnumerateDirectories(dir))
                        {
                            try
                            {
                                var di = new DirectoryInfo(sub);
                                if ((di.Attributes & (FileAttributes.System | FileAttributes.Hidden)) != 0) continue;
                                Scan(sub);
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            }

            Scan(rootPath);
            return results;
        }

        public static List<string> GetInvalidFiles(string rootPath, CheckBox cRootOnly)
        {
            var results = new List<string>();
            var root = new DirectoryInfo(rootPath);

            if ((root.Attributes & (FileAttributes.System | FileAttributes.Hidden)) != 0)
                return results;

            void Scan(string dir)
            {
                try
                {
                    foreach (var file in Directory.EnumerateFiles(dir))
                    {
                        try
                        {
                            var fi = new FileInfo(file);
                            if ((fi.Attributes & (FileAttributes.System | FileAttributes.Hidden)) != 0) continue;
                            if (fi.Extension is ".ini" or ".db" or ".com" or ".exe" or ".dll" or ".txt") continue;

                            if (!FrmPhotoSearchMoveHelpers.IsValidMediaBySniff(file))
                                results.Add(file);
                        }
                        catch { }
                    }

                    if (!cRootOnly.Checked)
                    {
                        foreach (var sub in Directory.EnumerateDirectories(dir))
                        {
                            try
                            {
                                var di = new DirectoryInfo(sub);
                                if ((di.Attributes & (FileAttributes.System | FileAttributes.Hidden)) != 0) continue;
                                Scan(sub);
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            }

            Scan(rootPath);
            return results;
        }

        public (string, string) RecuperaMeseAnnoDaNomeFile(string nomeFile, string file)
        {
            string anno = "1970";
            string mese = "01";
            bool matchFound = false;

            foreach (var entry in prefixToIndexMap)
            {
                if (!nomeFile.StartsWith(entry.Key, StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    var possibleAnno = nomeFile[entry.Value..(entry.Value + 4)];
                    var possibleMese = nomeFile[(entry.Value + 4)..(entry.Value + 6)];
                    var year = int.Parse(possibleAnno);

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
                (anno, mese) = clsDateExtractor.ExtractYearMonth(nomeFile);
                var parsedYear = int.Parse(anno);

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

        private void lblComment_Click(object sender, EventArgs e)
        {
            if (!isProcessing)
            {
                using FrmAbout about = new();
                about.ShowDialog();
            }
        }
    }
}