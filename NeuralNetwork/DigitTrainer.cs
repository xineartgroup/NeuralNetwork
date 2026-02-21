using System.Drawing.Imaging;

namespace NeuralNetwork
{
    /// <summary>
    /// Utility class for loading handwritten digit images and training the neural network
    /// </summary>
    public class DigitTrainer
    {
        private readonly EnhancedNeuralNetwork _network;
        private readonly string _trainingFolderPath;
        private readonly Random _random = new();

        // Image processing constants
        private const int ImageSize = 28;
        private const int PixelCount = ImageSize * ImageSize; // 784

        public DigitTrainer(EnhancedNeuralNetwork network, string trainingFolderPath)
        {
            _network = network ?? throw new ArgumentNullException(nameof(network));
            _trainingFolderPath = trainingFolderPath ?? throw new ArgumentNullException(nameof(trainingFolderPath));

            if (!Directory.Exists(trainingFolderPath))
                throw new DirectoryNotFoundException($"Training folder not found: {trainingFolderPath}");
        }

        /// <summary>
        /// Load all digit images from the training folder structure
        /// </summary>
        public (List<List<double>> inputs, List<List<double>> outputs, List<int> labels) LoadTrainingData()
        {
            var inputs = new List<List<double>>();
            var outputs = new List<List<double>>();
            var labels = new List<int>();

            // Process folders 0 through 9
            for (int digit = 0; digit <= 9; digit++)
            {
                string digitFolder = Path.Combine(_trainingFolderPath, digit.ToString());

                if (!Directory.Exists(digitFolder))
                {
                    Console.WriteLine($"Warning: Folder for digit {digit} not found at {digitFolder}");
                    continue;
                }

                // Get all PNG files in the digit folder
                var imageFiles = Directory.GetFiles(digitFolder, "*.png");
                Console.WriteLine($"Found {imageFiles.Length} images for digit {digit}");

                foreach (string imageFile in imageFiles)
                {
                    try
                    {
                        // Load and process the image
                        var (pixelValues, success) = LoadAndProcessImage(imageFile);

                        if (success)
                        {
                            inputs.Add(pixelValues);
                            outputs.Add(CreateOneHotOutput(digit));
                            labels.Add(digit);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing file {imageFile}: {ex.Message}");
                    }
                }
            }

            Console.WriteLine($"Total loaded images: {inputs.Count}");
            return (inputs, outputs, labels);
        }

        /// <summary>
        /// Load and preprocess a single image using fast unsafe bitmap access
        /// </summary>
        private (List<double> pixelValues, bool success) LoadAndProcessImage(string imagePath)
        {
            try
            {
                using var bitmap = new Bitmap(imagePath);

                // Verify image dimensions
                if (bitmap.Width != ImageSize || bitmap.Height != ImageSize)
                {
                    Console.WriteLine($"Warning: Image {imagePath} has dimensions {bitmap.Width}x{bitmap.Height}, expected 28x28. Resizing...");
                    return (ResizeAndProcessImage(imagePath), true);
                }

                var pixelValues = new List<double>(PixelCount);

                // Lock the bitmap bits for fast unsafe access
                Rectangle rect = new(0, 0, ImageSize, ImageSize);
                BitmapData data = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                unsafe
                {
                    byte* ptr = (byte*)data.Scan0;

                    for (int y = 0; y < ImageSize; y++)
                    {
                        byte* row = ptr + (y * data.Stride);

                        for (int x = 0; x < ImageSize; x++)
                        {
                            int pos = x * 4; // 4 bytes per pixel (ARGB)

                            // Read in BGRA order (typical for Windows bitmaps)
                            byte blue = row[pos];
                            byte green = row[pos + 1];
                            byte red = row[pos + 2];
                            // byte alpha = row[pos + 3]; // Alpha channel not needed for grayscale

                            // Convert to grayscale using luminance formula
                            double grayscale = (0.299 * red + 0.587 * green + 0.114 * blue) / 255.0;

                            // For MNIST-style images with white digits on black background,
                            // we want higher values for white pixels. If your images are 
                            // black on white, use (1.0 - grayscale) instead.
                            double normalized = grayscale;

                            pixelValues.Add(normalized);
                        }
                    }
                }

                bitmap.UnlockBits(data);
                return (pixelValues, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image {imagePath}: {ex.Message}");
                return ([], false);
            }
        }

        /// <summary>
        /// Resize and process images that aren't 28x28 (using fast pixel access)
        /// </summary>
        private List<double> ResizeAndProcessImage(string imagePath)
        {
            using var original = new Bitmap(imagePath);
            using var resized = new Bitmap(ImageSize, ImageSize);

            // Resize the image
            using (var graphics = Graphics.FromImage(resized))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(original, 0, 0, ImageSize, ImageSize);
            }

            var pixelValues = new List<double>(PixelCount);

            // Lock the resized bitmap for fast unsafe access
            Rectangle rect = new(0, 0, ImageSize, ImageSize);
            BitmapData data = resized.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

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
                        pixelValues.Add(grayscale);
                    }
                }
            }

            resized.UnlockBits(data);
            return pixelValues;
        }

        /// <summary>
        /// Create one-hot encoded output for a digit (0-9)
        /// </summary>
        private static List<double> CreateOneHotOutput(int digit)
        {
            var output = new List<double>(10);
            for (int i = 0; i < 10; i++)
            {
                output.Add(i == digit ? 1.0 : 0.0);
            }
            return output;
        }

        /// <summary>
        /// Normalize all inputs to have zero mean and unit variance
        /// </summary>
        public static void NormalizeInputs(List<List<double>> inputs)
        {
            if (inputs == null || inputs.Count == 0)
                return;

            int inputSize = inputs[0].Count;

            // Calculate mean for each feature
            var means = new double[inputSize];
            for (int i = 0; i < inputSize; i++)
            {
                means[i] = inputs.Average(x => x[i]);
            }

            // Calculate standard deviation for each feature
            var stdDevs = new double[inputSize];
            for (int i = 0; i < inputSize; i++)
            {
                double variance = inputs.Average(x => Math.Pow(x[i] - means[i], 2));
                stdDevs[i] = Math.Sqrt(variance + 1e-8); // Add small epsilon to avoid division by zero
            }

            // Normalize
            foreach (var input in inputs)
            {
                for (int i = 0; i < inputSize; i++)
                {
                    input[i] = (input[i] - means[i]) / stdDevs[i];
                }
            }
        }

        /// <summary>
        /// Augment training data with small variations to improve generalization
        /// </summary>
        public List<(List<double> Input, List<double> Output)> AugmentData(
            List<List<double>> inputs,
            List<List<double>> outputs,
            int augmentationFactor = 1)
        {
            var augmented = new List<(List<double>, List<double>)>();

            // Add original data
            for (int i = 0; i < inputs.Count; i++)
            {
                augmented.Add((inputs[i], outputs[i]));
            }

            // Create augmented versions
            for (int factor = 0; factor < augmentationFactor; factor++)
            {
                for (int i = 0; i < inputs.Count; i++)
                {
                    // Only augment if we have a valid image
                    if (inputs[i].Count == PixelCount)
                    {
                        var augmentedInput = AddRandomNoise(inputs[i], noiseLevel: 0.05);
                        augmented.Add((augmentedInput, outputs[i]));
                    }
                }
            }

            return augmented;
        }

        /// <summary>
        /// Add small random noise to input for data augmentation
        /// </summary>
        private List<double> AddRandomNoise(List<double> input, double noiseLevel)
        {
            var noisy = new List<double>(input.Count);
            foreach (var value in input)
            {
                double noise = (_random.NextDouble() * 2 - 1) * noiseLevel;
                noisy.Add(Math.Clamp(value + noise, 0, 1));
            }
            return noisy;
        }

        /// <summary>
        /// Split data into training and validation sets
        /// </summary>
        public (List<List<double>> trainInputs, List<List<double>> trainOutputs,
                List<List<double>> valInputs, List<List<double>> valOutputs)
            SplitData(List<List<double>> inputs, List<List<double>> outputs, double validationRatio = 0.2)
        {
            int validationCount = (int)(inputs.Count * validationRatio);
            var indices = Enumerable.Range(0, inputs.Count).ToList();

            // Shuffle indices
            for (int i = indices.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                (indices[i], indices[j]) = (indices[j], indices[i]);
            }

            var trainInputs = new List<List<double>>();
            var trainOutputs = new List<List<double>>();
            var valInputs = new List<List<double>>();
            var valOutputs = new List<List<double>>();

            for (int i = 0; i < indices.Count; i++)
            {
                int idx = indices[i];
                if (i < validationCount)
                {
                    valInputs.Add(inputs[idx]);
                    valOutputs.Add(outputs[idx]);
                }
                else
                {
                    trainInputs.Add(inputs[idx]);
                    trainOutputs.Add(outputs[idx]);
                }
            }

            return (trainInputs, trainOutputs, valInputs, valOutputs);
        }

        /// <summary>
        /// Calculate accuracy on validation data
        /// </summary>
        public double CalculateAccuracy(List<List<double>> inputs, List<List<double>> expectedOutputs)
        {
            int correct = 0;

            for (int i = 0; i < inputs.Count; i++)
            {
                var prediction = _network.FeedForward(inputs[i]);
                int predictedClass = prediction.IndexOf(prediction.Max());
                int actualClass = expectedOutputs[i].IndexOf(1.0);

                if (predictedClass == actualClass)
                    correct++;
            }

            return (double)correct / inputs.Count;
        }
    }
}