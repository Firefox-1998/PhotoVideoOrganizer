using System.Collections.Concurrent;

namespace PhotoMoveYearMonthFolder
{
    public partial class FrmPhotoSearchMove : Form
    {
        private readonly SemaphoreSlim semaphoreLock = new(1, 1);

        /*
        private Label[] Lbl_Desc = new Label[25]; // Crea un array di n Label
        private Label[] Lbl_FileNameProc = new Label[25]; // Crea un array di n Label
        */

        private string sSearchDir = "";
        private string sDestDir = "";
        private bool isProcessing;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private int processedFiles = 0;
        private readonly ConcurrentBag<Task> tasks = new(); // Usa ConcurrentBag invece di List o HashSet
        private ConcurrentDictionary<string, byte> fileHashes = new();

        public FrmPhotoSearchMove()
        {
            InitializeComponent();
            /*
            for (int i = 0; i < Lbl_Desc.Length; i++)
            {
                Lbl_Desc[i] = new Label
                {
                    Name = "Lbl_Desc" + i,
                    AutoSize = true,
                    Text = "File processed " + (i + 1).ToString("D2") + ":",
                    Location = new Point(12, (i * 20) + 127) // Posiziona le label verticalmente
                };
                Controls.Add(Lbl_Desc[i]); // Aggiungi la label alla form

                Lbl_FileNameProc[i] = new Label
                {
                    Name = "Lbl_FileNameProc" + i,
                    AutoSize = true,
                    Text = "-",
                    Location = new Point(110, (i * 20) + 127) // Posiziona le label verticalmente
                };
                Controls.Add(Lbl_FileNameProc[i]); // Aggiungi la label alla form
            } 
            */
        }

        private async void Btn_Start_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(sSearchDir) && !string.IsNullOrEmpty(sDestDir) && !sSearchDir.Equals(sDestDir))
            {
                Btn_DirDest.Enabled = false;
                Btn_DirSearch.Enabled = false;
                Btn_Start.Enabled = false;
                Btn_Cancel.Enabled = true;
                fileHashes = new();
                isProcessing = true;
                processedFiles = 0;

                try
                {
                    Logger.Log($">>> START <<<");
                    var semaphore = new SemaphoreSlim(10); // Imposta il numero massimo di thread a 10
                    var files = Directory.EnumerateFiles(sSearchDir, "*", SearchOption.AllDirectories).Where(FrmPhotoSearchMoveHelpers.IsValidFile).ToList();
                    int numFiles = files.Count;

                    LblNumFiles.Text = $"Num. file da processare: {numFiles}";
                    pbProcessFiles.Maximum = numFiles;

                    var tasks = files.Select((file, i) =>
                    {
                        /*
                        var lblFileNameProc = Lbl_FileNameProc[i % Lbl_FileNameProc.Length];
                        */

                        var lblFileNumProc = LblFileProc;                        
                        var progressbarNumFileProc = pbProcessFiles;
                        return Task.Run(async () =>
                        {
                            await semaphore.WaitAsync(_cancellationTokenSource.Token);
                            try
                            {
                                /*await ProcessFileAsync(file, lblFileNameProc, lblFileNumProc, progressbarNumFileProc);*/
                                await ProcessFileAsync(file, lblFileNumProc, progressbarNumFileProc);
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        }, _cancellationTokenSource.Token);
                    });

                    await Task.WhenAll(tasks);
                    Logger.Log($">>> END <<<");
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
                }

                Btn_DirDest.Enabled = true;
                Btn_DirSearch.Enabled = true;
                Btn_Start.Enabled = true;
                Btn_Cancel.Enabled = false;
                Btn_Cancel.Text = "Cancel";
                isProcessing = false;
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

        /*private async Task ProcessFileAsync(string file, Label lblFileNameProc, Label lblFileNumProc, ProgressBar pbNumFilesProc)*/
        private async Task ProcessFileAsync(string file, Label lblFileNumProc, ProgressBar pbNumFilesProc)
        {
            await semaphoreLock.WaitAsync();
            try
            {
                /*lblFileNameProc.Invoke((Action)(() => lblFileNameProc.Text = file.Length > 40 ? file[^40..] : file));*/

                string nomeFile = Path.GetFileNameWithoutExtension(file);
                string anno, mese;

                if (nomeFile.StartsWith("IMG-", StringComparison.OrdinalIgnoreCase) || nomeFile.StartsWith("VID-", StringComparison.OrdinalIgnoreCase))
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

        private void FrmPhotoSearchMove_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (tasks.Any(t => !t.IsCompleted) && isProcessing) // Controlla se ci sono task non completati e is processing è true
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
            _cancellationTokenSource.Cancel();
            Logger.Log($">>> CANCEL REQUEST !!! <<<");
            Btn_Cancel.Enabled = false;
            Btn_Cancel.Text = "Wait...";
        }
    }
}