using System.Globalization;
using static System.Windows.Forms.Design.AxImporter;

namespace PhotoMoveYearMonthFolder
{
    public partial class FrmPhotoSearchMove : Form
    {
        private SemaphoreSlim semaphoreLock = new(1, 1);

        private Label[] Lbl_Desc = new Label[25]; // Crea un array di n Label
        private Label[] Lbl_FileNameProc = new Label[25]; // Crea un array di n Label

        private string sSearchDir = "";
        private string sDestDir = "";
        private readonly object fileLock = new();
        private bool isProcessing = false;

        private int processedFiles = 0;

        public FrmPhotoSearchMove()
        {
            InitializeComponent();
            for (int i = 0; i < Lbl_Desc.Length; i++)
            {
                Lbl_Desc[i] = new Label
                {
                    Name = "Lbl_Desc" + i,
                    AutoSize = true,
                    Text = "File processed thread " + (i + 1).ToString("D2") + ":",
                    Location = new Point(12, (i * 20) + 127) // Posiziona le label verticalmente
                };
                this.Controls.Add(Lbl_Desc[i]); // Aggiungi la label alla form

                Lbl_FileNameProc[i] = new Label
                {
                    Name = "Lbl_FileNameProc" + i,
                    AutoSize = true,
                    Text = "-",
                    Location = new Point(150, (i * 20) + 127) // Posiziona le label verticalmente
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
                    var semaphore = new SemaphoreSlim(25); // Imposta il numero massimo di thread a 25
                    var files = Directory.EnumerateFiles(sSearchDir, "*", SearchOption.AllDirectories);
                    var tasks = new List<Task>();
                    long numFiles = files.Count();

                    LblNumFiles.Text = "Num. file da processare: " + numFiles;                    

                    int i = 0;
                    /*_ = Parallel.ForEach(files, options, (file) =>*/
                    _ = Parallel.ForEach(files, (file) =>
                    {                        
                        // Controlla se � un file immagine
                        if (IsValidFile(file))
                        {
                            var label = Lbl_FileNameProc[i % Lbl_FileNameProc.Length];
                            var label1 = LblFileProc;
                            tasks.Add(Task.Run(async () =>
                            {
                                await semaphore.WaitAsync();
                                try
                                {
                                    await ProcessFileAsync(file, label, label1);
                                }
                                finally
                                {
                                    _ = semaphore.Release();
                                }
                            }));
                            i++;
                        }
                    });
                    await Task.WhenAll(tasks);
                    MessageBox.Show("Elaborazione completata!", "Informazioni", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Si � verificato un errore: " + ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                Btn_DirDest.Enabled = true;
                Btn_DirSearch.Enabled = true;
                Btn_Start.Enabled = true;
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

        private static bool IsValidFile(string file)
        {
            // Ottieni l'estensione del file
            string extension = Path.GetExtension(file).ToLower();

            // Elenco di estensioni di file immagine
            string[] validExtensions = [".jpg", ".jpeg", ".png", ".bmp", ".gif", ".mp4"];

            // Restituisce true se l'estensione � presente nell'elenco
            return validExtensions.Contains(extension);
        }

        private static string GenerateNewFileName(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath) ?? throw new ArgumentNullException(nameof(filePath));
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string fileExtension = Path.GetExtension(filePath);

            int counter = 1;
            string newFileName = $"{fileNameWithoutExtension}_{counter}{fileExtension}";

            while (File.Exists(Path.Combine(directory, newFileName)))
            {
                counter++;
                newFileName = $"{fileNameWithoutExtension}_{counter}{fileExtension}";
            }

            return Path.Combine(directory, newFileName);
        }

        private async Task ProcessFileAsync(string file, Label label, Label label1)
        {

            // Acquisisci il semaforo (equivalente a entrare in un blocco 'lock')
            await semaphoreLock.WaitAsync();
            try
            {
                if (file.Length > 30)
                {
                    label.Invoke((Action)(() => label.Text = file.Substring(file.Length - 30, 30)));
                }
                else
                {
                    label.Invoke((Action)(() => label.Text = file));
                }                

                // Ottieni i primi sei caratteri del nome file
                string nomeFile = Path.GetFileNameWithoutExtension(file);
                string anno = nomeFile[..4];
                string mese = nomeFile.Substring(4, 2);

                if (anno.Equals("IMG-", StringComparison.OrdinalIgnoreCase) || anno.Equals("VID-", StringComparison.OrdinalIgnoreCase))
                {
                    anno = nomeFile.Substring(4, 4);
                    mese = nomeFile.Substring(8, 2);
                }

                if (DateTime.TryParseExact(anno + mese,
                                            "yyyyMM",
                                            provider: CultureInfo.InvariantCulture,
                                            DateTimeStyles.None,
                                            out _))
                {
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
                    string destinazioneFile = Path.Combine(cartellaMese, nomeFile + Path.GetExtension(file));

                    processedFiles = Interlocked.Increment(ref processedFiles);

                    //Copio il file verificando se gi� essite
                    //nel caso esista gi� rinomino il file che sto copiando
                    //solo nella cartella destinazione.
                    if (File.Exists(destinazioneFile))
                    {
                        string newFileName = GenerateNewFileName(destinazioneFile);
                        await CopyFileAsync(file, newFileName);
                    }
                    else
                    {
                        await CopyFileAsync(file, destinazioneFile);
                    }
                                        
                    label1.Invoke((Action)(() => label1.Text = "Num. file processati: " + processedFiles.ToString()));
                }
            }
            finally
            {
                // Rilascia il semaforo (equivalente a uscire da un blocco 'lock')
                semaphoreLock.Release();
            }
        }

        private void FrmPhotoSearchMove_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isProcessing)
            {
                e.Cancel = true;
                MessageBox.Show("Non � possibile chiudere la form durante l'elaborazione.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private async Task CopyFileAsync(string sourceFile, string destinationFile)
        {
            using FileStream sourceStream = new(sourceFile, FileMode.Open, FileAccess.Read);
            using var destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write);
            await sourceStream.CopyToAsync(destinationStream);
        }
    }
}
