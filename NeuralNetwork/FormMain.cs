using System.Diagnostics;
using static System.Windows.Forms.LinkLabel;

namespace NeuralNetwork
{
    public partial class FormMain : Form
    {
        private NeuralNetwork? model = null;
        private List<double> inputsFromImage = [];
        private string testImagePath = "";
        private bool isBlackOnWhiteSelected = false;

        private CancellationTokenSource? _cancellationTokenSource = null;
        private readonly object _pauseLock = new();

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            if (txtFIle.Text != "")
            {
                try
                {
                    string modelPath = Path.Combine(txtFIle.Text, "model.json");
                    if (File.Exists(modelPath))
                    {
                        model = NeuralNetwork.LoadFromFile(modelPath);
                        btnTrain.Text = "Continue Training";
                        btnRunInference.Enabled = true;
                    }
                    else
                    {
                        model = new NeuralNetwork(
                            layers:
                            [
                                new (784, ActivationFunctionType.Linear),    // Input layer: 784 (28x28 pixels)
                                new (128, ActivationFunctionType.ReLU),      // Hidden layer: 1 128 neurons
                                new (64, ActivationFunctionType.ReLU),       // Hidden layer: 2 64 neurons
                                new (10, ActivationFunctionType.Softmax)     // Output layer: 10 (digits 0-9)
                            ],
                            LossFunctionType.CrossEntropy
                        )
                        {
                            LearningRate = 0.01,
                            Momentum = 0.9,
                            WeightDecay = 0.0001
                        };
                        btnTrain.Text = "Start Training";
                        btnRunInference.Enabled = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    btnTrain.Enabled = false;
                    btnTrain.Text = "Error Loading Model";
                    MessageBox.Show($"Error loading model: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog ofd = new();

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtFIle.Text = ofd.SelectedPath;

                try
                {
                    string modelPath = Path.Combine(txtFIle.Text, "model.json");
                    if (File.Exists(modelPath))
                    {
                        model = NeuralNetwork.LoadFromFile(modelPath);
                        btnTrain.Text = "Continue Training";
                        btnRunInference.Enabled = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    MessageBox.Show($"Error loading model: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void BtnTrain_Click(object sender, EventArgs e)
        {
            btnTrain.Enabled = false;
            btnRunInference.Enabled = false;
            btnStopTraining.Enabled = true;

            _cancellationTokenSource = new CancellationTokenSource();

            await Task.Run(() => TrainAsync(_cancellationTokenSource.Token));
        }

        private void BtnStopTraining_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            btnStopTraining.Enabled = false;
            Console.WriteLine("Stopping training...");
            txtConsole.Text += "\r\nStatus: Stopping...";
        }

        private async Task TrainAsync(CancellationToken cancellationToken)
        {
            try
            {
                string trainingFolder = txtFIle.Text;

                Console.WriteLine("=== Handwritten Digit Recognition Neural Network ===");

                if (model == null)
                {
                    Console.WriteLine("No valid neural network was found.");
                    return;
                }

                var trainer = new TrainerDigits(model, trainingFolder);

                Console.WriteLine("Loading training data...");
                var stopwatch = Stopwatch.StartNew();

                var (inputs, outputs, labels) = trainer.LoadTrainingData();

                stopwatch.Stop();
                Console.WriteLine($"Loaded {inputs.Count} images in {stopwatch.ElapsedMilliseconds}ms");

                if (inputs.Count == 0)
                {
                    Console.WriteLine("No images found! Please check your folder structure.");
                    return;
                }

                Console.WriteLine("Class distribution:");
                for (int i = 0; i < 10; i++)
                {
                    int count = labels.Count(l => l == i);
                    Console.WriteLine($"Digit {i}: {count} images");
                }

                Console.WriteLine("Normalizing inputs...");
                TrainerDigits.NormalizeInputs(inputs);

                Console.WriteLine("Augmenting training data...");
                var augmentedData = trainer.AugmentData(inputs, outputs, augmentationFactor: 1);
                var augInputs = augmentedData.Select(x => x.Input).ToList();
                var augOutputs = augmentedData.Select(x => x.Output).ToList();

                Console.WriteLine($"Augmented dataset size: {augInputs.Count} images");

                Console.WriteLine("Splitting data into training and validation sets...");
                var (trainInputs, trainOutputs, valInputs, valOutputs) = trainer.SplitData(augInputs, augOutputs, validationRatio: 0.1);

                Console.WriteLine($"Training samples: {trainInputs.Count}");
                Console.WriteLine($"Validation samples: {valInputs.Count}");

                Console.WriteLine("Starting training...");

                double bestAccuracy = 0;

                var progress = new Progress<string>(msg =>
                {
                    if (txtConsole.InvokeRequired)
                    {
                        txtConsole.Invoke(new Action(() =>
                        {
                            txtConsole.AppendText(msg + " ");
                            txtConsole.ScrollToCaret();
                        }));
                    }
                    else
                    {
                        txtConsole.AppendText(msg + " ");
                    }
                    Console.Write(msg + " ");
                });

                string strCompletion = "Training cancelled";

                for (int epoch = 0; epoch < nEpochs.Value; epoch++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break; //cancellationToken.ThrowIfCancellationRequested();
                    }

                    stopwatch.Restart();

                    double loss = await Task.Run(() => 
                    model.Train(trainInputs, trainOutputs, progress: progress, cancellationToken: cancellationToken));

                    stopwatch.Stop();

                    double valAccuracy = trainer.CalculateAccuracy(valInputs, valOutputs);

                    if (valAccuracy > bestAccuracy)
                    {
                        bestAccuracy = valAccuracy;
                        model.SaveToFile(trainingFolder + "\\model.json");
                    }

                    Console.WriteLine();
                    Console.WriteLine(new string('-', 50));
                    Console.WriteLine("Epoch\tLoss\t\tVal Accuracy\tTime");
                    Console.WriteLine(new string('-', 50));
                    Console.WriteLine($"{epoch + 1}\t{loss:F6}\t{valAccuracy:P2}\t\t{stopwatch.ElapsedMilliseconds}ms");
                    Console.WriteLine(new string('-', 50));
                    Console.WriteLine();

                    if (valAccuracy > 0.999)
                    {
                        Console.WriteLine("Reached 99.9% validation accuracy! Stopping early.");
                        break;
                    }

                    strCompletion = "Training complete";
                }

                Console.WriteLine($"{strCompletion}! Best validation accuracy: {bestAccuracy:P2}");

                UpdateUI(() =>
                {
                    txtConsole.Text += $"\r\n{strCompletion}! Best validation accuracy: {bestAccuracy:P2}";
                });

                Console.WriteLine("Sample predictions:");
                TestRandomSamples(valInputs, valOutputs, 10);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\r\n\r\n" + ex.StackTrace);
                MessageBox.Show($"Error: {ex.Message}");
            }
            finally
            {
                UpdateUI(() =>
                {
                    btnRunInference.Enabled = model != null;
                    btnTrain.Enabled = true;
                    btnStopTraining.Enabled = false;
                });

                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private void UpdateUI(Action action)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(action);
            }
            else
            {
                action();
            }
        }

        private void RadioBlackOnWhite_CheckedChanged(object sender, EventArgs e)
        {
            if (sender != null)
            {
                if (sender is RadioButton)
                {
                    (inputsFromImage, _, isBlackOnWhiteSelected) = LoadSingleImage(testImagePath, isBlackOnWhite: radioBlackOnWhite.Checked);
                }
            }
        }

        private void BtnBrowseImage_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new()
                {
                    Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp",
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    testImagePath = openFileDialog.FileName;

                    Bitmap? bmp;
                    (inputsFromImage, bmp, isBlackOnWhiteSelected) = LoadSingleImage(testImagePath, isBlackOnWhite: radioBlackOnWhite.Checked);

                    if (isBlackOnWhiteSelected)
                    {
                        radioBlackOnWhite.Checked = true;
                    }
                    else
                    {
                        radioWhiteOnBlack.Checked = true;
                    }

                    if (bmp != null)
                    {
                        var old = pictureDigit.Image;
                        pictureDigit.Image = bmp;
                        old?.Dispose();

                        lblDigit.Text = "";

                        if (inputsFromImage != null && inputsFromImage.Count > 0)
                        {
                            Console.WriteLine($"Image: {Path.GetFileName(testImagePath)}");
                            Console.WriteLine("");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\r\n\r\n" + ex.StackTrace);
                MessageBox.Show($"Error loading image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRunInference_Click(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("=== RUNNING INFERENCE ===");

                if (model == null)
                {
                    MessageBox.Show("No valid neural network was found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (inputsFromImage != null && inputsFromImage.Count > 0)
                {
                    var prediction = model.FeedForward(inputsFromImage); // Run inference

                    Console.WriteLine("Prediction results:");
                    Console.WriteLine("");

                    for (int i = 0; i < 10; i++)
                    {
                        string bar = new('■', (int)(prediction[i] * 20));
                        Console.WriteLine($"Digit {i}: {prediction[i]:P2} {bar}");
                        txtConsole.Text += $"Digit {i}: {prediction[i]:P2} {bar}\r\n";
                    }

                    int predictedDigit = prediction.IndexOf(prediction.Max());
                    double confidence = prediction.Max();

                    Console.WriteLine("");
                    Console.WriteLine($"Predicted digit: {predictedDigit} with {confidence:P1} confidence");

                    txtConsole.Text += $"\r\nPredicted digit: {predictedDigit} with {confidence:P1} confidence\r\n";

                    lblDigit.Text = $"{predictedDigit}";

                    if (radioBlackOnWhite.Checked)
                    {
                        lblDigit.BackColor = Color.White;
                        lblDigit.ForeColor = Color.Black;
                    }
                    else
                    {
                        lblDigit.BackColor = Color.Black;
                        lblDigit.ForeColor = Color.White;
                    }
                }
                else
                {
                    Console.WriteLine("No image loaded for inference.");
                    MessageBox.Show("Please load an image first for inference.", "No Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\r\n\r\n" + ex.StackTrace);
                MessageBox.Show($"Error during inference: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private (List<double>, Bitmap?, bool) LoadSingleImage(string imagePath, bool isBlackOnWhite = false)
        {
            try
            {
                const int ImageSize = 28;
                const int PixelCount = ImageSize * ImageSize;

                using var bitmap = new Bitmap(imagePath);

                // Resize if necessary
                Bitmap processedBitmap;
                if (bitmap.Width != ImageSize || bitmap.Height != ImageSize)
                {
                    processedBitmap = new Bitmap(ImageSize, ImageSize);
                    using var graphics = Graphics.FromImage(processedBitmap);
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.DrawImage(bitmap, 0, 0, ImageSize, ImageSize);
                }
                else
                {
                    processedBitmap = new Bitmap(bitmap);
                }

                var pixelValues = new List<double>(PixelCount);

                // Lock the bitmap bits for fast unsafe access
                var rect = new Rectangle(0, 0, ImageSize, ImageSize);
                var data = processedBitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                int lightCount = 0;
                int darkCount = 0;

                unsafe
                {
                    byte* ptr = (byte*)data.Scan0;

                    for (int y = 0; y < ImageSize; y++)
                    {
                        byte* row = ptr + (y * data.Stride);

                        for (int x = 0; x < ImageSize; x++)
                        {
                            int pos = x * 4;

                            byte blue = row[pos];
                            byte green = row[pos + 1];
                            byte red = row[pos + 2];

                            double grayscale = (0.299 * red + 0.587 * green + 0.114 * blue) / 255.0;
                            double normalized = grayscale;

                            if (normalized > 0.5)
                            {
                                lightCount++;
                            }
                            else
                            {
                                darkCount++;
                            }

                            if (isBlackOnWhite)
                            {
                                pixelValues.Add(1.0 - normalized); // Invert for black-on-white
                            }
                            else
                            {
                                pixelValues.Add(normalized);
                            }
                        }
                    }
                }

                processedBitmap.UnlockBits(data);
                //processedBitmap.Dispose();

                return (pixelValues, processedBitmap, lightCount > darkCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image {imagePath}: {ex.Message}");
                return ([], null, false);
            }
        }

        private void TestRandomSamples(List<List<double>> inputs, List<List<double>> outputs, int sampleCount)
        {
            if (model != null)
            {
                var random = new Random();

                for (int i = 0; i < sampleCount; i++)
                {
                    int idx = random.Next(inputs.Count);
                    var input = inputs[idx];
                    var expected = outputs[idx];

                    var prediction = model.FeedForward(input);
                    int predictedDigit = prediction.IndexOf(prediction.Max());
                    int actualDigit = expected.IndexOf(1.0);

                    // Show top 3 predictions with confidence
                    var topPredictions = prediction
                        .Select((value, index) => new { Digit = index, Confidence = value })
                        .OrderByDescending(x => x.Confidence)
                        .Take(3);

                    Console.Write("  Top predictions: ");
                    foreach (var p in topPredictions)
                    {
                        Console.Write($"{p.Digit}({p.Confidence:P1}) ");
                    }
                }
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                var result = MessageBox.Show("Training is still in progress. Stop and exit?",
                    "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _cancellationTokenSource.Cancel();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
