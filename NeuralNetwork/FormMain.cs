using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NeuralNetwork
{
    public partial class FormMain : Form
    {
        private EnhancedNeuralNetwork? nn = null;
        private List<double> inputsFromImage = [];
        private string testImagePath = "";
        private bool isBlackOnWhiteSelected = false;

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
                        nn = EnhancedNeuralNetwork.LoadFromFile(modelPath);
                        btnTrain.Text = "Continue Training";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    MessageBox.Show($"Error loading model: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            if (nn != null)
            {
                btnTrain.Text = "Continue Training";
                btnRunInference.Enabled = true;
            }
            else
            {
                btnTrain.Text = "Start Training";
                btnRunInference.Enabled = false;
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
                        nn = EnhancedNeuralNetwork.LoadFromFile(modelPath);
                        btnTrain.Text = "Continue Training";
                        btnRunInference.Enabled = true;
                    }
                    else
                    {
                        double learningRate = 0.01;

                        nn = new EnhancedNeuralNetwork(
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
                            // Set hyperparameters
                            LearningRate = learningRate,
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
                    MessageBox.Show($"Error loading model: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnTrain_Click(object sender, EventArgs e)
        {
            try
            {
                btnTrain.Enabled = false;
                btnRunInference.Enabled = false;

                // Configuration
                string trainingFolder = txtFIle.Text;
                int batchSize = 32;
                int epochs = 20;
                double validationRatio = 0.2;

                Console.WriteLine("=== Handwritten Digit Recognition Neural Network ===");

                if (nn == null)
                {
                    Console.WriteLine("No valid neural network was found.");
                    return;
                }

                // Create trainer
                var trainer = new DigitTrainer(nn, trainingFolder);

                // Load training data
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

                // Check class distribution
                Console.WriteLine("Class distribution:");
                for (int i = 0; i < 10; i++)
                {
                    int count = labels.Count(l => l == i);
                    Console.WriteLine($"Digit {i}: {count} images");
                }

                // Optional: Normalize inputs
                Console.WriteLine("Normalizing inputs...");
                DigitTrainer.NormalizeInputs(inputs);

                // Optional: Augment data
                Console.WriteLine("Augmenting training data...");
                var augmentedData = trainer.AugmentData(inputs, outputs, augmentationFactor: 1);
                var augInputs = augmentedData.Select(x => x.Input).ToList();
                var augOutputs = augmentedData.Select(x => x.Output).ToList();

                Console.WriteLine($"Augmented dataset size: {augInputs.Count} images");

                // Split into training and validation sets
                Console.WriteLine("Splitting data into training and validation sets...");
                var (trainInputs, trainOutputs, valInputs, valOutputs) =
                    trainer.SplitData(augInputs, augOutputs, validationRatio);

                Console.WriteLine($"Training samples: {trainInputs.Count}");
                Console.WriteLine($"Validation samples: {valInputs.Count}");

                // Train the network
                Console.WriteLine("Starting training...");

                double bestAccuracy = 0;

                for (int epoch = 0; epoch < epochs; epoch++)
                {
                    stopwatch.Restart();

                    // Train one epoch
                    double loss = nn.Train(trainInputs, trainOutputs, batchSize, epochs: 1);

                    stopwatch.Stop();

                    // Calculate validation accuracy
                    double valAccuracy = trainer.CalculateAccuracy(valInputs, valOutputs);

                    // Track best model
                    if (valAccuracy > bestAccuracy)
                    {
                        bestAccuracy = valAccuracy;
                        // Save best model
                        nn.SaveToFile(trainingFolder + "\\model.json");
                    }

                    // Progress report
                    Console.WriteLine("Epoch\tLoss\t\tVal Accuracy\tTime");
                    Console.WriteLine(new string('-', 50));
                    Console.WriteLine($"{epoch + 1}\t{loss:F6}\t{valAccuracy:P2}\t\t{stopwatch.ElapsedMilliseconds}ms");

                    // Early stopping if we reach high accuracy
                    if (valAccuracy > 0.99)
                    {
                        Console.WriteLine("Reached 99% validation accuracy! Stopping early.");
                        break;
                    }
                }

                Console.WriteLine($"Training complete! Best validation accuracy: {bestAccuracy:P2}");

                // Test on some validation samples
                Console.WriteLine("Sample predictions:");
                TestRandomSamples(valInputs, valOutputs, 10);

                // Save final model
                nn.SaveToFile(trainingFolder + "\\model.json");
                Console.WriteLine("Model saved to 'model.json'");
                txtConsole.Text += $"\r\nTraining complete! Best validation accuracy: {bestAccuracy:P2}";
                txtConsole.Text += "\r\nModel saved to 'model.json'";
                btnRunInference.Enabled = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\r\n\r\n" + ex.StackTrace);
                MessageBox.Show($"Error: {ex.Message}");
            }
            finally
            {
                btnRunInference.Enabled = nn != null;
                btnTrain.Enabled = true;
            }
        }

        private void RadioBlackOnWhite_CheckedChanged(object sender, EventArgs e)
        {
            if (sender != null)
            {
                if (sender is RadioButton)
                {
                    if (sender is RadioButton radio)
                    {
                        if (isBlackOnWhiteSelected != radioBlackOnWhite.Checked)
                        {
                            (inputsFromImage, _, isBlackOnWhiteSelected) = LoadSingleImage(testImagePath, isBlackOnWhite: radioBlackOnWhite.Checked);
                        }
                    }
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

                    if (isBlackOnWhiteSelected != radioBlackOnWhite.Checked)
                    {
                        (inputsFromImage, bmp, isBlackOnWhiteSelected) = LoadSingleImage(testImagePath, isBlackOnWhite: isBlackOnWhiteSelected);
                    }

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

                if (nn == null)
                {
                    MessageBox.Show("No valid neural network was found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (inputsFromImage != null && inputsFromImage.Count > 0)
                {
                    var prediction = nn.FeedForward(inputsFromImage); // Run inference

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
            if (nn != null)
            {
                var random = new Random();

                for (int i = 0; i < sampleCount; i++)
                {
                    int idx = random.Next(inputs.Count);
                    var input = inputs[idx];
                    var expected = outputs[idx];

                    var prediction = nn.FeedForward(input);
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
    }
}
