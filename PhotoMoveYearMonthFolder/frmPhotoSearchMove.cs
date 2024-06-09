using System.Collections.Concurrent;
using System.Globalization;
using Windows.Security.Authentication.Identity.Provider;

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
        private readonly Dictionary<string, int> prefixToIndexMap = new Dictionary<string, int>
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
                    //var files = Directory.EnumerateFiles(sSearchDir, "*", SearchOption.AllDirectories).Where(FrmPhotoSearchMoveHelpers.IsValidFile).ToList();
                    var files = GetValidFiles(sSearchDir);
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

                    // Processo i file che "NON" hanno un'estensione valida jpg, jpeg, ecc.
                    // e li copio nella directory "OtherFilesExt"
                    //files = Directory.EnumerateFiles(sSearchDir, "*", SearchOption.AllDirectories).Where(file => !FrmPhotoSearchMoveHelpers.IsValidFile(file)).ToList();                    
                    files = GetInvalidFiles(sSearchDir);
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
                (string anno, string mese) = RecuperaMeseAnnoDaNomeFile(nomeFile, file);

                /*
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
                    if (nomeFile.StartsWith("Screenshot_", StringComparison.OrdinalIgnoreCase)) 
                    {
                        anno = nomeFile[11..15];
                        mese = nomeFile[15..17];
                    }
                    else
                    {
                        if (nomeFile.StartsWith("VideoCapture_", StringComparison.OrdinalIgnoreCase))
                        {
                            anno = nomeFile[13..17];
                            mese = nomeFile[17..19];
                        }
                        else
                        {
                            if (nomeFile.StartsWith("IMG", StringComparison.OrdinalIgnoreCase) ||
                                nomeFile.StartsWith("VID", StringComparison.OrdinalIgnoreCase) ||
                                nomeFile.StartsWith("WP_", StringComparison.OrdinalIgnoreCase)
                                )
                            {
                                anno = nomeFile[3..7];
                                mese = nomeFile[7..9];
                            }
                            else
                            {
                                string parsedDate = FrmPhotoSearchMoveHelpers.ReadExifData(file);
                                anno = parsedDate[..4];
                                mese = parsedDate[4..6];
                            }
                        }
                    }
                }
                */

                string cartellaAnno = Path.Combine(sDestDir, anno);
                Directory.CreateDirectory(cartellaAnno);

                string cartellaMese = Path.Combine(cartellaAnno, mese);
                Directory.CreateDirectory(cartellaMese);

                string destinazioneFile = Path.Combine(cartellaMese, nomeFile + Path.GetExtension(file));
                string fileHash = FrmPhotoSearchMoveHelpers.ComputeHash(file);
                bool fileExists = File.Exists(destinazioneFile);
                bool areFilesIdentical = fileExists && fileHashes.ContainsKey(fileHash);

                string fileUniqueImageID = FrmPhotoSearchMoveHelpers.ReadExifUniqueImageID(file);
                string destinationFileUniqueImageID = "";
                if (fileExists)
                {
                    destinationFileUniqueImageID = FrmPhotoSearchMoveHelpers.ReadExifUniqueImageID(destinazioneFile);
                }

                bool sameUniqueImageID = (!string.IsNullOrEmpty(fileUniqueImageID) || 
                    !string.IsNullOrEmpty(destinationFileUniqueImageID)) && fileUniqueImageID == destinationFileUniqueImageID;

                if (!areFilesIdentical)
                {
                    if (!sameUniqueImageID)
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
                        Logger.Log($"Saltato. Stesso TAG EXIF 'ImageUniqueID' {file} {destinazioneFile}");
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

        /*
        public static List<string> GetValidFiles(string rootPath)
        {
            var validFiles = new List<string>();

            foreach (var directory in Directory.EnumerateDirectories(rootPath))
            {
                var directoryInfo = new DirectoryInfo(directory);

                // Salta le directory di sistema o nascoste
                if ((directoryInfo.Attributes & FileAttributes.System) == FileAttributes.System ||
                    (directoryInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                // Aggiungi i file validi alla lista
                validFiles.AddRange(Directory.EnumerateFiles(directory).Where(FrmPhotoSearchMoveHelpers.IsValidFile));

                // Ricorsione nelle sottodirectory
                validFiles.AddRange(GetValidFiles(directory));
            }
            return validFiles;
        }
        */

        public static List<string> GetValidFiles(string rootPath)
        {
            var validFiles = new List<string>();

            // Aggiungi i file validi nella directory radice alla lista
            validFiles.AddRange(Directory.EnumerateFiles(rootPath).Where(FrmPhotoSearchMoveHelpers.IsValidFile));

            foreach (var directory in Directory.EnumerateDirectories(rootPath))
            {
                var directoryInfo = new DirectoryInfo(directory);

                // Salta le directory di sistema o nascoste
                if ((directoryInfo.Attributes & FileAttributes.System) == FileAttributes.System ||
                    (directoryInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                // Aggiungi i file validi alla lista
                validFiles.AddRange(Directory.EnumerateFiles(directory).Where(FrmPhotoSearchMoveHelpers.IsValidFile));

                // Ricorsione nelle sottodirectory
                validFiles.AddRange(GetValidFiles(directory));
            }
            return validFiles;
        }

        /*
        public static List<string> GetInvalidFiles(string rootPath)
        {
            var invalidFiles = new List<string>();

            foreach (var directory in Directory.EnumerateDirectories(rootPath))
            {
                var directoryInfo = new DirectoryInfo(directory);

                // Salta le directory di sistema o nascoste
                if ((directoryInfo.Attributes & FileAttributes.System) == FileAttributes.System ||
                    (directoryInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                // Aggiungi i file non validi alla lista
                invalidFiles.AddRange(Directory.EnumerateFiles(directory).Where(file => !FrmPhotoSearchMoveHelpers.IsValidFile(file)));

                // Ricorsione nelle sottodirectory
                invalidFiles.AddRange(GetInvalidFiles(directory));
            }
            return invalidFiles;
        }
        

        public static List<string> GetInvalidFiles(string rootPath)
        {
            var invalidFiles = new List<string>();

            // Aggiungi i file non validi nella directory radice alla lista
            invalidFiles.AddRange(Directory.EnumerateFiles(rootPath).Where(file => !FrmPhotoSearchMoveHelpers.IsValidFile(file)));

            foreach (var directory in Directory.EnumerateDirectories(rootPath))
            {
                var directoryInfo = new DirectoryInfo(directory);

                // Salta le directory di sistema o nascoste
                if ((directoryInfo.Attributes & FileAttributes.System) == FileAttributes.System ||
                    (directoryInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                // Aggiungi i file non validi alla lista
                invalidFiles.AddRange(Directory.EnumerateFiles(directory).Where(file => !FrmPhotoSearchMoveHelpers.IsValidFile(file)));

                // Ricorsione nelle sottodirectory
                invalidFiles.AddRange(GetInvalidFiles(directory));
            }
            return invalidFiles;
        }
        */
        public static List<string> GetInvalidFiles(string rootPath)
        {
            var invalidFiles = new List<string>();

            foreach (var directory in Directory.EnumerateDirectories(rootPath))
            {
                var directoryInfo = new DirectoryInfo(directory);

                // Salta le directory di sistema o nascoste
                if ((directoryInfo.Attributes & FileAttributes.System) == FileAttributes.System ||
                    (directoryInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    continue;
                }

                // Aggiungi i file non validi alla lista
                invalidFiles.AddRange(Directory.EnumerateFiles(directory).Where(file =>
                {
                    var fileInfo = new FileInfo(file);

                    // Salta i file di sistema o nascosti
                    if ((fileInfo.Attributes & FileAttributes.System) == FileAttributes.System ||
                        (fileInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden ||
                        fileInfo.Extension == ".ini" || fileInfo.Extension == ".db" ||
                        fileInfo.Extension == ".com" || fileInfo.Extension == ".exe" ||
                        fileInfo.Extension == ".dll" || fileInfo.Extension == ".txt")
                    {
                        return false;
                    }

                    // Verifica se il file è valido
                    return !FrmPhotoSearchMoveHelpers.IsValidFile(file);
                }));

                // Ricorsione nelle sottodirectory
                invalidFiles.AddRange(GetInvalidFiles(directory));
            }

            return invalidFiles;
        }

        public (string, string) RecuperaMeseAnnoDaNomeFile(string nomeFile, string file)
        {
            string anno = string.Empty;
            string mese = string.Empty;
            bool matchFound = false;

            foreach (var entry in prefixToIndexMap)
            {
                if (nomeFile.StartsWith(entry.Key, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        anno = nomeFile[entry.Value..(entry.Value + 4)];
                        mese = nomeFile[(entry.Value + 4)..(entry.Value + 6)];
                        matchFound = true;
                        break; // Uscire dal ciclo non appena si trova una corrispondenza
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        continue; // continua il ciclo a causa errore sulla lunghezza del nome file
                    }
                }
            }

            if (!matchFound)
            {
                (anno, mese) = clsDateExtractor.ExtractYearMonth(nomeFile);
            }

            // Se nessuna corrispondenza viene trovata nel dizionario, eseguire l'azione predefinita
            if (!matchFound && (int.Parse(anno) < 1970 || int.Parse(anno) > DateTime.Now.Year))
            {
                string parsedDate = FrmPhotoSearchMoveHelpers.ReadExifData(file);
                anno = parsedDate[..4];
                mese = parsedDate[4..6];
            }

            if (int.Parse(anno) < 1970 || int.Parse(anno) > DateTime.Now.Year)
            {
                anno = "1970";
                mese = "01";
            }
            return (anno, mese);
        }
    }
}