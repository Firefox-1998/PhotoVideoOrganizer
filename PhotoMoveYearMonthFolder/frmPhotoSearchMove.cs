using System.Collections.Concurrent;

namespace PhotoMoveYearMonthFolder
{
    public partial class FrmPhotoSearchMove : Form
    {
        private const string SuffixLogFile = "yyyyMMdd-HHmmss";
        private readonly SemaphoreSlim semaphoreLock = new(1, 1);
        private string sSearchDir = "";
        private string sDestDir = "";
        private bool isProcessing;
        private CancellationTokenSource? _cancellationTokenSource;
        private int processedFiles = 0;
        private int processedOtherFiles = 0;
        private ConcurrentDictionary<string, byte> fileHashes = new();

        public FrmPhotoSearchMove()
        {
            InitializeComponent();
        }

        private async void Btn_Start_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(sSearchDir) && !string.IsNullOrEmpty(sDestDir) && !sSearchDir.Equals(sDestDir))
            {
                _cancellationTokenSource = new();
                Logger.SetLogFilePath(sDestDir + "\\" + DateTime.Now.ToString(SuffixLogFile) + "_PhotoSearchCopyLog.txt");
                tbMaxThread.Enabled = false;
                Btn_DirDest.Enabled = false;
                Btn_DirSearch.Enabled = false;
                Btn_Start.Enabled = false;
                Btn_Cancel.Enabled = true;                
                isProcessing = true;
                processedFiles = 0;

                try
                {
                    fileHashes = new();
                    // Processo i file con estensione valida jpg, jpeg, ecc
                    Logger.Log($">>> START VALID EXT<<<");
                    var semaphore = new SemaphoreSlim(tbMaxThread.Value); // Imposta il numero massimo di thread in base a quanto definito dall'utente (MIN: 1 - MAX: 20)
                    var files = Directory.EnumerateFiles(sSearchDir, "*", SearchOption.AllDirectories).Where(FrmPhotoSearchMoveHelpers.IsValidFile).ToList();
                    int numFiles = files.Count;

                    LblNumFiles.Text = $"Num. file da processare: {numFiles}";
                    pbProcessFiles.Maximum = numFiles;

                    var tasks = files.Select((file, i) =>
                    {
                        var lblFileNumProc = LblFileProc;
                        var progressbarNumFileProc = pbProcessFiles;
                        return Task.Run(async () =>
                        {
                            await semaphore.WaitAsync(_cancellationTokenSource.Token);
                            try
                            {
                                await ProcessFileAsync(file, lblFileNumProc, progressbarNumFileProc);
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        }, _cancellationTokenSource.Token);
                    });

                    await Task.WhenAll(tasks);                    
                    Logger.Log($">>> END VALID EXT <<<");

                    // Processo i file con "NON" hanno un'estensione valida jpg, jpeg, ecc.
                    // e li copio nella directory "OtherFilesExt"
                    files = Directory.EnumerateFiles(sSearchDir, "*", SearchOption.AllDirectories).Where(file => !FrmPhotoSearchMoveHelpers.IsValidFile(file)).ToList();
                    numFiles = files.Count;
                    LblNumOtherFiles.Text = $"Num. altri file da processare: {numFiles}";                    
                    if (numFiles != 0)
                    {
                        fileHashes = new();
                        pbProcessedOtherFiles.Maximum = numFiles;
                        Logger.Log($"\r\n---------------------------------\r\n");
                        Logger.Log($">>> START >> NOT << VALID EXT<<<");
                        tasks = files.Select((file, i) =>
                        {
                            var lblOtherFileNumProc = LblOtherFileProc;
                            var progressbarNumOtherFileProc = pbProcessedOtherFiles;
                            return Task.Run(async () =>
                            {
                                await semaphore.WaitAsync(_cancellationTokenSource.Token);
                                try
                                {
                                    await ProcessFileAsyncNotValidExt(file, lblOtherFileNumProc, progressbarNumOtherFileProc);
                                }
                                finally
                                {
                                    semaphore.Release();
                                }
                            }, _cancellationTokenSource.Token);
                        });

                        await Task.WhenAll(tasks);                        
                        Logger.Log($">>> END > NOT < VALID EXT <<<");
                    }
                    MessageBox.Show("Elaborazione completata!", "Informazioni", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    if (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        Logger.Log($">>> ERRORE: {ex.Message} <<<");
                        MessageBox.Show($"Si è verificato un errore: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                finally
                {
                    _cancellationTokenSource.Dispose();
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
                    if (Btn_Cancel.Enabled)
                    {
                        Btn_Cancel.Enabled = false;
                    }
                    else
                    {
                        Btn_Cancel.Text = "Cancel";
                    }
                    isProcessing = false;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(sSearchDir) || string.IsNullOrEmpty(sDestDir) || sSearchDir.Equals(sDestDir))
                {
                    MessageBox.Show($"Verificare la corretezza delle directory di origine e destinazione.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
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
            await semaphoreLock.WaitAsync();
            try
            {
                string nomeFile = Path.GetFileNameWithoutExtension(file);
                string anno, mese;

                if (nomeFile.StartsWith("IMG-", StringComparison.OrdinalIgnoreCase) ||
                    nomeFile.StartsWith("IMG_", StringComparison.OrdinalIgnoreCase) ||
                    nomeFile.StartsWith("VID-", StringComparison.OrdinalIgnoreCase) ||
                    nomeFile.StartsWith("AUD-", StringComparison.OrdinalIgnoreCase) ||
                    nomeFile.StartsWith("PPT-", StringComparison.OrdinalIgnoreCase))
                {
                    anno = nomeFile[4..8];
                    mese = nomeFile[8..10];
                }
                else
                {
                    string parsedDate = FrmPhotoSearchMoveHelpers.ReadExifData(file);
                    anno = parsedDate[..4];
                    mese = parsedDate[4..6];
                }

                if (int.Parse(anno) < 1970)
                {
                    anno = "1970";
                    mese = "01";
                }

                string cartellaAnno = Path.Combine(sDestDir, anno);
                Directory.CreateDirectory(cartellaAnno);

                string cartellaMese = Path.Combine(cartellaAnno, mese);
                Directory.CreateDirectory(cartellaMese);

                string destinazioneFile = Path.Combine(cartellaMese, nomeFile + Path.GetExtension(file));
                string fileHash = FrmPhotoSearchMoveHelpers.ComputeHash(file);
                bool fileExists = File.Exists(destinazioneFile);
                bool areFilesIdentical = fileExists && fileHashes.ContainsKey(fileHash);

                if (!areFilesIdentical)
                {
                    if (fileExists)
                    {
                        // Calcola l'hash del file esistente
                        string existingFileHash = FrmPhotoSearchMoveHelpers.ComputeHash(destinazioneFile);

                        if (fileHash != existingFileHash)
                        {
                            // I file sono diversi, quindi copia il file con un nuovo nome
                            string destinationFile = FrmPhotoSearchMoveHelpers.GenerateNewFileName(destinazioneFile);
                            await FrmPhotoSearchMoveHelpers.CopyFileAsync(file, destinationFile);
                            Logger.Log($"Copiato {file} {destinationFile}");
                            fileHashes.TryAdd(fileHash, 0);
                        }
                        else
                        {
                            // I file sono identici, quindi salta la copia del file
                            Logger.Log($"Saltato {file} {destinazioneFile}");
                        }
                    }
                    else
                    {
                        // Il file non esiste, quindi copia il file
                        await FrmPhotoSearchMoveHelpers.CopyFileAsync(file, destinazioneFile);
                        Logger.Log($"Copiato {file} {destinazioneFile}");
                        fileHashes.TryAdd(fileHash, 0);
                    }
                }
                else
                {
                    Logger.Log($"Saltato {file} {destinazioneFile}");
                }
            }
            catch (FormatException)
            {
                Logger.Log($"Eccezione formato data {file}");
            }
            finally
            {
                processedFiles = Interlocked.Increment(ref processedFiles);
                lblFileNumProc.Invoke((Action)(() => lblFileNumProc.Text = $"Num. file processati: {processedFiles}"));
                pbNumFilesProc.Invoke((Action)(() => pbNumFilesProc.Value = processedFiles));

                semaphoreLock.Release();
            }
        }

        private async Task ProcessFileAsyncNotValidExt(string file, Label lblOtherFileNumProc, ProgressBar pbNumOtherFilesProc)
        {
            await semaphoreLock.WaitAsync();
            try
            {
                string nomeFile = Path.GetFileNameWithoutExtension(file);
                string cartellaOtherExt = Path.Combine(sDestDir, "OtherFilesExt");
                Directory.CreateDirectory(cartellaOtherExt);

                string destinazioneFile = Path.Combine(cartellaOtherExt, nomeFile + Path.GetExtension(file));
                string fileHash = FrmPhotoSearchMoveHelpers.ComputeHash(file);
                bool fileExists = File.Exists(destinazioneFile);
                bool areFilesIdentical = fileExists && fileHashes.ContainsKey(fileHash);

                if (!areFilesIdentical)
                {
                    if (fileExists)
                    {
                        // Calcola l'hash del file esistente
                        string existingFileHash = FrmPhotoSearchMoveHelpers.ComputeHash(destinazioneFile);

                        if (fileHash != existingFileHash)
                        {
                            // I file sono diversi, quindi copia il file con un nuovo nome
                            string destinationFile = FrmPhotoSearchMoveHelpers.GenerateNewFileName(destinazioneFile);
                            await FrmPhotoSearchMoveHelpers.CopyFileAsync(file, destinationFile);
                            Logger.Log($"Copiato {file} {destinationFile}");
                            fileHashes.TryAdd(fileHash, 0);
                        }
                        else
                        {
                            // I file sono identici, quindi salta la copia del file
                            Logger.Log($"Saltato {file} {destinazioneFile}");
                        }
                    }
                    else
                    {
                        // Il file non esiste, quindi copia il file
                        await FrmPhotoSearchMoveHelpers.CopyFileAsync(file, destinazioneFile);
                        Logger.Log($"Copiato {file} {destinazioneFile}");
                        fileHashes.TryAdd(fileHash, 0);
                    }
                }
                else
                {
                    Logger.Log($"Saltato {file} {destinazioneFile}");
                }
            }
            catch (FormatException)
            {
                Logger.Log($"Eccezione formato data {file}");
            }
            finally
            {
                processedOtherFiles = Interlocked.Increment(ref processedOtherFiles);
                lblOtherFileNumProc.Invoke((Action)(() => lblOtherFileNumProc.Text = $"Num. file processati: {processedOtherFiles}"));
                pbNumOtherFilesProc.Invoke((Action)(() => pbNumOtherFilesProc.Value = processedOtherFiles));
                semaphoreLock.Release();
            }
        }

        private void FrmPhotoSearchMove_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isProcessing) // Controlla se ci sono task non completati e isProcessing è true
            {
                e.Cancel = true;
                MessageBox.Show("Non è possibile chiudere la form durante l'elaborazione.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                Logger.Log($">>> EXIT <<<");
            }
        }

        private void Btn_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult Cancelrequest = MessageBox.Show("Confermi l'interruzione dell'elaborazione?", "Info", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (Cancelrequest == DialogResult.Yes)
            {
                _cancellationTokenSource?.Cancel();
                Logger.Log($">>> CANCEL REQUEST !!! <<<");
                Btn_Cancel.Enabled = false;
                Btn_Cancel.Text = "CANCEL REQUEST\r\nWait...";
            }
        }

        private void TbMaxThread_Scroll(object sender, EventArgs e)
        {
            lblMaxThread.Text = "Max Thread: " + tbMaxThread.Value.ToString();
        }
    }
}