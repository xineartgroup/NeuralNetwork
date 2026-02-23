namespace NeuralNetwork
{
    public class TrainerDigits
    {
        private readonly NeuralNetwork _model;
        private readonly Random _random = new();

        public TrainerDigits(NeuralNetwork model, string trainingFolderPath)
        {
            _model = model ?? throw new ArgumentNullException(nameof(model));

            if (!Directory.Exists(trainingFolderPath))
                throw new DirectoryNotFoundException($"Training folder not found: {trainingFolderPath}");
        }

        public static (List<List<double>> inputs, List<List<double>> outputs, List<int> labels) LoadTrainingData(string folderPath)
        {
            var inputs = new List<List<double>>();
            var outputs = new List<List<double>>();
            var labels = new List<int>();

            // Process folders 0 through 9
            for (int digit = 0; digit <= 9; digit++)
            {
                string digitFolder = Path.Combine(folderPath, digit.ToString());

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
                        var (pixelValues, success) = ImageManip.LoadAndProcessImage(imageFile);

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

        private static List<double> CreateOneHotOutput(int digit)
        {
            var output = new List<double>(10);
            for (int i = 0; i < 10; i++)
            {
                output.Add(i == digit ? 1.0 : 0.0);
            }
            return output;
        }

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

        public List<(List<double> Input, List<double> Output)> AugmentData(List<List<double>> inputs, List<List<double>> outputs, int augmentationFactor = 1)
        {
            var augmented = new List<(List<double>, List<double>)>();

            // 1. Add original data
            for (int i = 0; i < inputs.Count; i++)
            {
                augmented.Add((inputs[i], outputs[i]));
            }

            // 2. Create augmented versions
            Random rand = new();
            for (int factor = 0; factor < augmentationFactor; factor++)
            {
                for (int i = 0; i < inputs.Count; i++)
                {
                    if (inputs[i].Count == ImageManip.PixelCount)
                    {
                        var noisyInput = AddRandomNoise(inputs[i], noiseLevel: 0.05);
                        augmented.Add((noisyInput, outputs[i]));

                        var rotatedInput = RotateImage(inputs[i], angleDegrees: (rand.NextDouble() * 30) - 15);
                        augmented.Add((rotatedInput, outputs[i]));
                    }
                }
            }

            return augmented;
        }

        private static List<double> RotateImage(List<double> pixels, double angleDegrees)
        {
            int size = (int)Math.Sqrt(pixels.Count); // Assuming square image (e.g., 28x28)
            double[] result = new double[pixels.Count];
            double angleRad = angleDegrees * Math.PI / 180.0;
            double center = (size - 1) / 2.0;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    double relX = x - center;
                    double relY = y - center;

                    int sourceX = (int)Math.Round(relX * Math.Cos(-angleRad) - relY * Math.Sin(-angleRad) + center);
                    int sourceY = (int)Math.Round(relX * Math.Sin(-angleRad) + relY * Math.Cos(-angleRad) + center);

                    if (sourceX >= 0 && sourceX < size && sourceY >= 0 && sourceY < size)
                    {
                        result[y * size + x] = pixels[sourceY * size + sourceX];
                    }
                    else
                    {
                        result[y * size + x] = 0; // Padding with black/empty space
                    }
                }
            }
            return [.. result];
        }

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

        public double CalculateAccuracy(List<List<double>> inputs, List<List<double>> expectedOutputs)
        {
            int correct = 0;

            for (int i = 0; i < inputs.Count; i++)
            {
                var prediction = _model.FeedForward(inputs[i]);
                int predictedClass = prediction.IndexOf(prediction.Max());
                int actualClass = expectedOutputs[i].IndexOf(1.0);

                if (predictedClass == actualClass)
                    correct++;
            }

            return (double)correct / inputs.Count;
        }
    }
}