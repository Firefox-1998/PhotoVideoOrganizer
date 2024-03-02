using ExifLib;
using System.Globalization;
using System.Security.Cryptography;


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
                    var semaphore = new SemaphoreSlim(15); // Imposta il numero massimo di thread a 15
                    var files = Directory.EnumerateFiles(sSearchDir, "*", SearchOption.AllDirectories);
                    var tasks = new List<Task>();
                    long numFiles = files.Count();

                    LblNumFiles.Text = "Num. file da processare: " + numFiles;

                    int i = 0;
                    /*_ = Parallel.ForEach(files, options, (file) =>*/
                    _ = Parallel.ForEach(files, (file) =>
                    {
                        // Controlla se è un file immagine
                        if (IsValidFile(file))
                        {
                            var label = Lbl_FileNameProc[i % Lbl_FileNameProc.Length];
                            var label1 = LblFileProc;
                            tasks.Add(Task.Run(async () =>
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
                            }));
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

        private static bool IsValidFile(string file)
        {
            // Ottieni l'estensione del file
            string extension = Path.GetExtension(file).ToLower();

            // Elenco di estensioni di file immagine
            string[] validExtensions = [".jpg", ".jpeg", ".png", ".bmp", ".gif", ".mp4"];

            // Restituisce true se l'estensione è presente nell'elenco
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

                if (!DateTime.TryParseExact(anno + mese,
                                            "yyyyMM",
                                            provider: CultureInfo.InvariantCulture,
                                            DateTimeStyles.None,
                                            out _))
                {
                    // Leggi i dati EXIF
                    ReadExifData(file, out DateTime parsedDate);                    
                    anno = parsedDate.ToString("yyyyMMdd")[..4];
                    mese = parsedDate.ToString("yyyyMMdd").Substring(4, 2);
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
                processedFiles = Interlocked.Increment(ref processedFiles);

                if (File.Exists(destinazioneFile))
                {
                    // Se il file che devo copiare è
                    // identico al file che già esiste (calcolo HASH dei file)
                    // NON eseguo la copia.
                    if (!FilesAreIdentical(file, destinazioneFile))
                    {
                        string newFileName = GenerateNewFileName(destinazioneFile);
                        await CopyFileAsync(file, newFileName);
                    }
                }
                else
                {
                    await CopyFileAsync(file, destinazioneFile);
                }

                label1.Invoke((Action)(() => label1.Text = "Num. file processati: " + processedFiles.ToString()));
                
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
                MessageBox.Show("Non è possibile chiudere la form durante l'elaborazione.", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static async Task CopyFileAsync(string sourceFile, string destinationFile)
        {
            FileStream sourceStream = new(sourceFile, FileMode.Open, FileAccess.Read);
            var destinationStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write);
            await sourceStream.CopyToAsync(destinationStream);
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

        private static void ReadExifData(string image, out DateTime parsedDate)
        {
            try
            {
                var reader = new ExifReader(image);

                reader.GetTagValue(ExifTags.DateTime, out DateTime date);                
                bool isDate = DateTime.TryParse(date.ToString(), provider: CultureInfo.InvariantCulture,
                                            DateTimeStyles.None,
                                            out _);

                if (isDate)
                {
                    parsedDate = DateTime.ParseExact(date.ToString(),
                                                         "yyyyMMdd",
                                                         provider: CultureInfo.InvariantCulture,
                                                         style: DateTimeStyles.None);
                }
                else
                {
                    reader.GetTagValue(ExifTags.DateTimeOriginal, out DateTime dateoriginal);
                    isDate = DateTime.TryParse(dateoriginal.ToString(), provider: CultureInfo.InvariantCulture,
                                            DateTimeStyles.None,
                                            out _);
                    if (isDate)
                    {
                        parsedDate = DateTime.ParseExact(dateoriginal.ToString(),
                                                         "yyyyMMdd",
                                                         provider: CultureInfo.InvariantCulture,
                                                         style: DateTimeStyles.None);

                    }
                    else 
                    {
                        parsedDate = DateTime.ParseExact("19700101",
                                                         "yyyyMMdd",
                                                         provider: CultureInfo.InvariantCulture,
                                                         style: DateTimeStyles.None);
                    }
                }                    
             }
            catch (ExifLibException)
            {
                parsedDate = DateTime.ParseExact("19700101",
                                                 "yyyyMMdd",
                                                 provider: CultureInfo.InvariantCulture,
                                                 style: DateTimeStyles.None);
                return;
            }
        }

        private bool FilesAreIdentical(string path1, string path2)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] hash1 = GetFileHash(sha256, path1);
            byte[] hash2 = GetFileHash(sha256, path2);

            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1[i] != hash2[i])
                {
                    return false;
                }
            }

            return true;
        }

        private byte[] GetFileHash(SHA256 sha256, string path)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                return sha256.ComputeHash(stream);
            }
        }
    }
}