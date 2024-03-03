using System.Globalization;


namespace PhotoMoveYearMonthFolder
{
    public partial class FrmPhotoSearchMove : Form
    {
        private SemaphoreSlim semaphoreLock = new(1, 1);

        private Label[] Lbl_Desc = new Label[25]; // Crea un array di n Label
        private Label[] Lbl_FileNameProc = new Label[25]; // Crea un array di n Label

        private string sSearchDir = "";
        private string sDestDir = "";
        private bool isProcessing;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private int processedFiles = 0;
        private List<Task> tasks = new();

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
                this.Controls.Add(Lbl_Desc[i]); // Aggiungi la label alla form

                Lbl_FileNameProc[i] = new Label
                {
                    Name = "Lbl_FileNameProc" + i,
                    AutoSize = true,
                    Text = "-",
                    Location = new Point(110, (i * 20) + 127) // Posiziona le label verticalmente
                };
                this.Controls.Add(Lbl_FileNameProc[i]); // Aggiungi la label alla form
            }            
        }

        private async void Btn_Start_Click(object sender, EventArgs e)
        {
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
                    long numFiles = files.Count();

                    LblNumFiles.Text = "Num. file da processare: " + numFiles;

                    int i = 0;
                    /*_ = Parallel.ForEach(files, options, (file) =>*/
                    _ = Parallel.ForEach(files, (file) =>
                    {
                        // Controlla se è un file immagine
                        if (frmPhotoSearchMoveHelpers.IsValidFile(file))
                        {
                            var label = Lbl_FileNameProc[i % Lbl_FileNameProc.Length];
                            var label1 = LblFileProc;
                            var task = Task.Run(async () =>
                            {
                                await semaphore.WaitAsync(_cancellationTokenSource.Token);
                                try
                                {
                                    await ProcessFileAsync(file, label, label1);
                                }
                                finally
                                {
                                    _ = semaphore.Release();
                                }
                            });
                            tasks.Add(task);
                            task.ContinueWith(t => tasks.Remove(t));  // Rimuove il task dalla lista quando è completato
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

        private async Task ProcessFileAsync(string file, Label label, Label label1)
        {
            // Acquisisci il semaforo (equivalente a entrare in un blocco 'lock')
            await semaphoreLock.WaitAsync();
            try
            {
                if (file.Length > 40)
                {
                    label.Invoke((Action)(() => label.Text = file.Substring(file.Length - 40, 40)));
                }
                else
                {
                    label.Invoke((Action)(() => label.Text = file));
                }

                // Imposto il nome del file da processare
                string nomeFile = Path.GetFileNameWithoutExtension(file);

                // Prendo i primi sei caratteri del nome file per determinare la data di scatto dell'immagine
                // I primi quattro caratteri sono l'anno
                // I successivi due caratteri il mese
                string anno = nomeFile[..4];
                string mese = nomeFile.Substring(4, 2);

                // Se i primi quattro caratteri sono "IMG-" o "VID-"
                // allora prendo i successivi sei caratteri
                // ed imposto anno e mese
                if (anno.Equals("IMG-", StringComparison.OrdinalIgnoreCase) || anno.Equals("VID-", StringComparison.OrdinalIgnoreCase))
                {
                    anno = nomeFile.Substring(4, 4);
                    mese = nomeFile.Substring(8, 2);
                }

                // Verifico se anno e mese sono una data e ne caso non lo siano
                // tento di estrarre i dati Exif dall'immagine
                // per impostare anno e mese
                if (!DateTime.TryParseExact(anno + mese,
                                            "yyyyMM",
                                            provider: CultureInfo.InvariantCulture,
                                            DateTimeStyles.None,
                                            out _))
                {
                    // Leggi i dati EXIF
                    // Di default anno e mese verrrano impostati a
                    // anno = "1970"
                    // mese = "01"
                    // Verranno utilizzati questi dati
                    // nel caso in cui data scatto
                    // e data original non siano presenti
                    string parsedDate =frmPhotoSearchMoveHelpers.ReadExifData(file);                    
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

                if (File.Exists(destinazioneFile))
                {
                    // Se il file che devo copiare è
                    // identico al file che già esiste (calcolo HASH dei file)
                    // NON eseguo la copia.
                    bool areFilesIdentical = false;
                    int tentativi = 0;
                    while (tentativi < 5)
                    {
                        try
                        {
                            areFilesIdentical = frmPhotoSearchMoveHelpers.FilesAreIdentical(file, destinazioneFile);
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
                        processedFiles = Interlocked.Increment(ref processedFiles);
                        string newFileName = frmPhotoSearchMoveHelpers.GenerateNewFileName(destinazioneFile);
                        await frmPhotoSearchMoveHelpers.CopyFileAsync(file, newFileName);
                        Logger.Log("Copiato " + file + " " + newFileName + " ");
                    }
                    else
                    {
                        Logger.Log("Saltato " + file + " " + destinazioneFile + " ");
                    }
                }
                else
                {
                    processedFiles = Interlocked.Increment(ref processedFiles);
                    await frmPhotoSearchMoveHelpers.CopyFileAsync(file, destinazioneFile);
                    Logger.Log("Copiato " + file + " " + destinazioneFile + " ");
                }                
                
                label1.Invoke((Action)(() => label1.Text = "Num. file processati: " + processedFiles.ToString()));
            }
            catch (FormatException)
            {
                Logger.Log("Eccezione formato data " + file);
            }
            finally
            {
                // Rilascia il semaforo (equivalente a uscire da un blocco 'lock')
                semaphoreLock.Release();
            }
        }

        private void FrmPhotoSearchMove_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (tasks.Any(t => !t.IsCompleted))  // Controlla se ci sono task non completati
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