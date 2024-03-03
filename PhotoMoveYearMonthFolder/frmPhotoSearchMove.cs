using System.Collections.Concurrent;
using System.Globalization;


namespace PhotoMoveYearMonthFolder
{
    public partial class FrmPhotoSearchMove : Form
    {
        private readonly SemaphoreSlim semaphoreLock = new(1, 1);

        private Label[] Lbl_Desc = new Label[25]; // Crea un array di n Label
        private Label[] Lbl_FileNameProc = new Label[25]; // Crea un array di n Label

        private string sSearchDir = "";
        private string sDestDir = "";
        private bool isProcessing;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private int processedFiles = 0;
        private readonly ConcurrentBag<Task> tasks = []; // Usa ConcurrentBag invece di List o HashSet
        private HashSet<string> fileHashes;

        public FrmPhotoSearchMove()
        {
            InitializeComponent();

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
        }

        private async void Btn_Start_Click(object sender, EventArgs e)
        {
            fileHashes = [];
            isProcessing = true;
            if (!string.IsNullOrEmpty(sSearchDir) && !string.IsNullOrEmpty(sDestDir) && !sSearchDir.Equals(sDestDir))
            {
                Btn_DirDest.Enabled = false;
                Btn_DirSearch.Enabled = false;
                Btn_Start.Enabled = false;
                Btn_Cancel.Enabled = true;
                processedFiles = 0;

                /*
                ParallelOptions options = new ParallelOptions
                {
                    MaxDegreeOfParallelism = 20
                };
                */

                // Elabora tutte le immagini nella cartella e nelle sottocartelle
                try
                {
                    var semaphore = new SemaphoreSlim(10); // Imposta il numero massimo di thread a 10
                    var files = Directory.EnumerateFiles(sSearchDir, "*", SearchOption.AllDirectories);                    
                    int numFiles = files.Count();

                    LblNumFiles.Text = "Num. file da processare: " + numFiles;
                    pbProcessFiles.Maximum = numFiles;

                    int i = 0;
                    /*_ = Parallel.ForEach(files, options, (file) =>*/
                    _ = Parallel.ForEach(files, (file) =>
                    {
                        // Controlla se è un file immagine
                        if (FrmPhotoSearchMoveHelpers.IsValidFile(file))
                        {
                            var lblFileNameProc = Lbl_FileNameProc[i % Lbl_FileNameProc.Length];
                            var lblFileNumProc = LblFileProc;
                            var progressbarNumFileProc = pbProcessFiles;
                            var task = Task.Run(async () =>
                            {
                                await semaphore.WaitAsync(_cancellationTokenSource.Token);
                                try
                                {
                                    await ProcessFileAsync(file, lblFileNameProc, lblFileNumProc, progressbarNumFileProc);
                                }
                                finally
                                {
                                    _ = semaphore.Release();
                                }
                            });
                            tasks.Add(task);
                            task.ContinueWith(t =>
                            {
                                while (!tasks.TryTake(out _)) { } // Rimuove il task dalla lista quando è completato
                            });
                            i++;
                        }
                    });
                    await Task.WhenAll(tasks);
                    MessageBox.Show("Elaborazione completata!", "Informazioni", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Si è verificato un errore: " + ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally 
                {
                    _cancellationTokenSource.Dispose();
                }

                Btn_DirDest.Enabled = true;
                Btn_DirSearch.Enabled = true;
                Btn_Start.Enabled = true;
                Btn_Cancel.Enabled = false;
            }
            isProcessing = false;
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

        private async Task ProcessFileAsync(string file, Label lblFileNameProc, Label lblFileNumProc, ProgressBar pbNumFilesProc)
        {
            // Acquisisci il semaforo (equivalente a entrare in un blocco 'lock')
            await semaphoreLock.WaitAsync();
            try
            {
                if (file.Length > 40)
                {
                    lblFileNameProc.Invoke((Action)(() => lblFileNameProc.Text = file.Substring(file.Length - 40, 40)));
                }
                else
                {
                    lblFileNameProc.Invoke((Action)(() => lblFileNameProc.Text = file));
                }

                // Imposto il nome del file da processare
                string nomeFile = Path.GetFileNameWithoutExtension(file);
                string anno;
                string mese;

                // Se i primi quattro caratteri sono "IMG-" o "VID-"
                // allora prendo i successivi sei caratteri
                // ed imposto anno e mese
                if (nomeFile[..4].Equals("IMG-", StringComparison.OrdinalIgnoreCase) || nomeFile[..4].Equals("VID-", StringComparison.OrdinalIgnoreCase))
                {
                    anno = nomeFile.Substring(4, 4);
                    mese = nomeFile.Substring(8, 2);
                }
                else
                {
                    // Leggi i dati EXIF
                    // Di default anno e mese verrrano impostati a
                    // anno = "1970"
                    // mese = "01"
                    // Verranno utilizzati questi dati
                    // nel caso in cui il nome del file non contiene
                    // anno e mese o data scatto e
                    // data original non siano presenti
                    string parsedDate = FrmPhotoSearchMoveHelpers.ReadExifData(file);
                    anno = parsedDate[..4];
                    mese = parsedDate.Substring(4, 2);
                }

                // Crea la cartella anno se non esiste
                string cartellaAnno = Path.Combine(sDestDir, anno);
                if (!Directory.Exists(cartellaAnno))
                {
                    Directory.CreateDirectory(cartellaAnno);
                }

                // Crea la cartella mese se non esiste
                string cartellaMese = Path.Combine(cartellaAnno, mese);
                if (!Directory.Exists(cartellaMese))
                {
                    Directory.CreateDirectory(cartellaMese);
                }

                // Copia il file nella cartella mese
                // verificando se già esiste e
                // nel caso esista già rinomino il file
                // nella cartella destinazione.
                // Il file di origine resta invariato
                string destinazioneFile = Path.Combine(cartellaMese, nomeFile + Path.GetExtension(file));
                string fileHash = FrmPhotoSearchMoveHelpers.ComputeHash(file);
                bool fileExists = File.Exists(destinazioneFile);
                bool areFilesIdentical = fileExists && fileHashes.Contains(fileHash);

                if (!areFilesIdentical)
                {
                    string destinationFile = fileExists ? FrmPhotoSearchMoveHelpers.GenerateNewFileName(destinazioneFile) : destinazioneFile;
                    await FrmPhotoSearchMoveHelpers.CopyFileAsync(file, destinationFile);
                    Logger.Log($"Copiato {file} {destinationFile}");
                    fileHashes.Add(fileHash);
                }
                else
                {
                    Logger.Log($"Saltato {file} {destinazioneFile}");
                }

                /*
                if (File.Exists(destinazioneFile))
                {
                    // Se il file che devo copiare è
                    // identico al file che già esiste (calcolo HASH dei file)
                    // NON eseguo la copia.                    
                    int tentativi = 0;
                    while (tentativi < 5)
                    {
                        try
                        {
                            areFilesIdentical = FrmPhotoSearchMoveHelpers.FilesAreIdentical(file, destinazioneFile);
                            break;  // Uscire dal ciclo se FilesAreIdentical non lancia un'eccezione
                        }
                        catch (IOException)
                        {
                            Logger.Log("Eccezione file in uso " + file + " " + destinazioneFile + " ");
                            // Aspetta un po' prima di riprovare
                            await Task.Delay(1000);
                            tentativi++;
                        }
                    }

                    if (!areFilesIdentical)
                    {                        
                        string newFileName = FrmPhotoSearchMoveHelpers.GenerateNewFileName(destinazioneFile);
                        await FrmPhotoSearchMoveHelpers.CopyFileAsync(file, newFileName);
                        Logger.Log("Copiato " + file + " " + newFileName + " ");
                        fileHashes.Add(fileHash);
                    }
                    else
                    {                        
                        Logger.Log("Saltato " + file + " " + destinazioneFile + " ");
                    }
                }
                else
                {                    
                    await FrmPhotoSearchMoveHelpers.CopyFileAsync(file, destinazioneFile);
                    Logger.Log("Copiato " + file + " " + destinazioneFile + " ");
                    fileHashes.Add(fileHash);
                }
                */
            }
            catch (FormatException)
            {
                Logger.Log($"Eccezione formato data {file}");
            }
            finally
            {
                processedFiles = Interlocked.Increment(ref processedFiles);
                lblFileNumProc.Invoke((Action)(() => lblFileNumProc.Text = "Num. file processati: " + processedFiles.ToString()));
                pbNumFilesProc.Invoke((Action)(() => pbNumFilesProc.Value = processedFiles));

                // Rilascia il semaforo (equivalente a uscire da un blocco 'lock')
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
        }

        private void Btn_Cancel_Click(object sender, EventArgs e)
        {
            Btn_DirDest.Enabled = true;
            Btn_DirSearch.Enabled = true;
            Btn_Start.Enabled = true;
            Btn_Cancel.Enabled = false;
            isProcessing = false;
            _cancellationTokenSource.Cancel();
        }
    }
}