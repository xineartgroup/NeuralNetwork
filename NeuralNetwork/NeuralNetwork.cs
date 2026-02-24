using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeuralNetwork
{
    [Serializable]
    public class NeuralNetwork
    {
        [JsonInclude]
        public List<NeuralNetworkLayer> Layers { get; private set; } = [];

        [JsonInclude]
        public List<Matrix> Weights { get; private set; } = [];

        [JsonInclude]
        public List<Matrix> Biases { get; private set; } = [];

        [JsonInclude]
        public double LearningRate { get; set; } = 0.01;

        [JsonInclude]
        public double Momentum { get; set; } = 0.9;

        [JsonInclude]
        public double WeightDecay { get; set; } = 0.0001;

        [JsonInclude]
        public LossFunctionType LossFunction { get; set; } = LossFunctionType.CrossEntropy;

        private readonly List<Matrix> _previousWeightGradients = [];
        private readonly List<Matrix> _previousBiasGradients = [];

        private readonly Random _random = new();

        private static readonly JsonSerializerOptions options = new()
        {
            WriteIndented = true,
            Converters = { new MatrixConverter() }
        };

        public NeuralNetwork()
        {
            Layers = [];
            Weights = [];
            Biases = [];
            _previousWeightGradients = [];
            _previousBiasGradients = [];
        }

        [JsonConstructor]
        public NeuralNetwork(List<NeuralNetworkLayer> layers, LossFunctionType lossFunction)
        {
            if (layers == null || layers.Count < 2)
                throw new ArgumentException("Network must have at least 2 layers");

            Layers = layers;
            LossFunction = lossFunction;

            InitializeNetwork();
        }

        private void InitializeNetwork()
        {
            Weights.Clear();
            Biases.Clear();
            _previousWeightGradients.Clear();
            _previousBiasGradients.Clear();

            for (int i = 0; i < Layers.Count - 1; i++)
            {
                int currentLayerSize = Layers[i].LayerSize;
                int nextLayerSize = Layers[i + 1].LayerSize;

                double scale = GetInitializationScale(i);

                var weightMatrix = new Matrix(nextLayerSize, currentLayerSize);
                for (int row = 0; row < nextLayerSize; row++)
                {
                    for (int col = 0; col < currentLayerSize; col++)
                    {
                        weightMatrix[row, col] = (_random.NextDouble() * 2 - 1) * scale;
                    }
                }
                Weights.Add(weightMatrix);

                var biasMatrix = new Matrix(nextLayerSize, 1);
                for (int row = 0; row < nextLayerSize; row++)
                {
                    biasMatrix[row, 0] = 0.01;
                }
                Biases.Add(biasMatrix);

                _previousWeightGradients.Add(new Matrix(nextLayerSize, currentLayerSize));
                _previousBiasGradients.Add(new Matrix(nextLayerSize, 1));
            }
        }

        private double GetInitializationScale(int layerIndex)
        {
            if (Layers[layerIndex + 1].ActivationFunction == ActivationFunctionType.ReLU ||
                Layers[layerIndex + 1].ActivationFunction == ActivationFunctionType.LeakyReLU)
            {
                return Math.Sqrt(2.0 / Layers[layerIndex].LayerSize);
            }
            return Math.Sqrt(1.0 / Layers[layerIndex].LayerSize);
        }

        public List<double> FeedForward(List<double> inputs)
        {
            if (inputs.Count != Layers[0].LayerSize)
                throw new ArgumentException($"Input size {inputs.Count} doesn't match network input size {Layers[0]}");

            var current = Matrix.FromList(inputs, false);

            for (int i = 0; i < Weights.Count; i++)
            {
                var z = Matrix.Multiply(Weights[i], current);
                z = Matrix.Add(z, Biases[i]);
                current = ApplyActivation(z, i);
            }

            return current.ToList();
        }

        private (List<Matrix> Activations, List<Matrix> ZValues) FeedForwardWithCache(Matrix input)
        {
            var activations = new List<Matrix> { input };
            var zValues = new List<Matrix>();

            var current = input;

            for (int i = 0; i < Weights.Count; i++)
            {
                var z = Matrix.Multiply(Weights[i], current);
                z = Matrix.Add(z, Biases[i]);
                zValues.Add(z);
                current = ApplyActivation(z, i);
                activations.Add(current);
            }

            return (activations, zValues);
        }

        public double Train(List<List<double>> inputs, List<List<double>> expectedOutputs, IProgress<string> progress, CancellationToken cancellationToken)
        {
            int batchSize = 32;

            if (inputs.Count != expectedOutputs.Count)
                throw new ArgumentException("Number of inputs must match number of expected outputs");

            if (inputs.Count == 0)
                throw new ArgumentException("Training data cannot be empty");

            var trainingData = new List<(List<double> Input, List<double> Output)>();

            for (int i = 0; i < inputs.Count; i++)
            {
                trainingData.Add((inputs[i], expectedOutputs[i]));
            }

            Shuffle(trainingData);

            double epochLoss = 0;
            int batches = 0;

            int lastPercent = 0;

            for (int i = 0; i < trainingData.Count; i += batchSize)
            {
                var batch = trainingData.Skip(i).Take(batchSize).ToList();
                if (batch.Count > 0)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break; //cancellationToken.ThrowIfCancellationRequested();
                    }

                    epochLoss += TrainBatch(batch);
                    int percent = (int)(i * 100.0 / trainingData.Count) + 1;
                    if (lastPercent < percent)
                    {
                        progress.Report($"{percent}% ");
                        lastPercent = percent;
                    }
                    batches++;
                }
            }

            return epochLoss / batches;
        }

        private double TrainBatch(List<(List<double> Input, List<double> Output)> batch)
        {
            var weightGradients = new List<Matrix>();
            var biasGradients = new List<Matrix>();

            for (int i = 0; i < Weights.Count; i++)
            {
                weightGradients.Add(new Matrix(Weights[i].Rows, Weights[i].Cols));
                biasGradients.Add(new Matrix(Biases[i].Rows, Biases[i].Cols));
            }

            double totalLoss = 0;

            foreach (var (input, expected) in batch)
            {
                var inputMatrix = Matrix.FromList(input, false);
                var expectedMatrix = Matrix.FromList(expected, false);

                var (activations, zValues) = FeedForwardWithCache(inputMatrix);

                totalLoss += CalculateLoss(activations[^1], expectedMatrix);

                var delta = CalculateOutputLayerDelta(activations[^1], expectedMatrix, zValues[^1]);

                for (int i = Weights.Count - 1; i >= 0; i--)
                {
                    var weightGrad = Matrix.Multiply(delta, Matrix.Transpose(activations[i]));
                    weightGradients[i] = Matrix.Add(weightGradients[i], weightGrad);

                    biasGradients[i] = Matrix.Add(biasGradients[i], delta);

                    if (i > 0)
                    {
                        delta = Matrix.Multiply(Matrix.Transpose(Weights[i]), delta);
                        delta = ElementWiseMultiply(delta, CalculateActivationDerivative(zValues[i - 1], i - 1));
                    }
                }
            }

            ApplyGradients(weightGradients, biasGradients, batch.Count);

            return totalLoss / batch.Count;
        }

        private static Matrix ElementWiseMultiply(Matrix a, Matrix b)
        {
            if (a.Rows != b.Rows || a.Cols != b.Cols)
                throw new ArgumentException("Matrices must have the same dimensions for element-wise multiplication");

            var result = new Matrix(a.Rows, a.Cols);
            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < a.Cols; j++)
                {
                    result[i, j] = a[i, j] * b[i, j];
                }
            }
            return result;
        }

        private void ApplyGradients(List<Matrix> weightGradients, List<Matrix> biasGradients, int batchSize)
        {
            double learningRate = LearningRate / batchSize;

            for (int i = 0; i < Weights.Count; i++)
            {
                var weightUpdate = Matrix.Multiply(weightGradients[i], learningRate);
                weightUpdate = Matrix.Add(weightUpdate, Matrix.Multiply(_previousWeightGradients[i], Momentum));

                var biasUpdate = Matrix.Multiply(biasGradients[i], learningRate);
                biasUpdate = Matrix.Add(biasUpdate, Matrix.Multiply(_previousBiasGradients[i], Momentum));

                if (WeightDecay > 0)
                {
                    var regularization = Matrix.Multiply(Weights[i], WeightDecay * learningRate);
                    weightUpdate = Matrix.Add(weightUpdate, regularization);
                }

                Weights[i] = Matrix.Subtract(Weights[i], weightUpdate);
                Biases[i] = Matrix.Subtract(Biases[i], biasUpdate);

                _previousWeightGradients[i] = weightUpdate;
                _previousBiasGradients[i] = biasUpdate;
            }
        }

        private Matrix CalculateOutputLayerDelta(Matrix output, Matrix expected, Matrix z)
        {
            if (LossFunction == LossFunctionType.CrossEntropy)
            {
                return Matrix.Subtract(output, expected);
            }
            else
            {
                var error = Matrix.Subtract(output, expected);
                var derivative = CalculateActivationDerivative(z, Weights.Count - 1);
                return ElementWiseMultiply(error, derivative);
            }
        }

        private Matrix ApplyActivation(Matrix input, int layer)
        {
            switch (Layers[layer + 1].ActivationFunction)
            {
                case ActivationFunctionType.Sigmoid:
                    return Matrix.Map(input, x => 1.0 / (1.0 + Math.Exp(-x)));
                case ActivationFunctionType.Tanh:
                    return Matrix.Map(input, x => Math.Tanh(x));
                case ActivationFunctionType.ReLU:
                    return Matrix.Map(input, x => Math.Max(0, x));
                case ActivationFunctionType.LeakyReLU:
                    return Matrix.Map(input, x => x > 0 ? x : 0.01 * x);
                case ActivationFunctionType.Linear:
                    return Matrix.Map(input, x => x);
                case ActivationFunctionType.Softmax:
                    return Softmax(input);
                default:
                    return input.Copy();
            }
        }

        private Matrix CalculateActivationDerivative(Matrix input, int layer)
        {
            switch (Layers[layer + 1].ActivationFunction)
            {
                case ActivationFunctionType.Sigmoid:
                    var sigmoid = ApplyActivation(input, layer);
                    return Matrix.Map(sigmoid, x => x * (1 - x));
                case ActivationFunctionType.Tanh:
                    return Matrix.Map(input, x => 1 - Math.Pow(Math.Tanh(x), 2));
                case ActivationFunctionType.ReLU:
                    return Matrix.Map(input, x => x > 0 ? 1.0 : 0.0);
                case ActivationFunctionType.LeakyReLU:
                    return Matrix.Map(input, x => x > 0 ? 1.0 : 0.01);
                case ActivationFunctionType.Linear:
                    return Matrix.Map(input, x => 1.0);
                case ActivationFunctionType.Softmax:
                    return Matrix.Map(input, x => 1.0);
                default:
                    return Matrix.Map(input, x => 1.0);
            }
        }

        private static Matrix Softmax(Matrix input)
        {
            var result = new Matrix(input.Rows, input.Cols);
            double sum = 0;
            double max = double.MinValue;

            for (int i = 0; i < input.Rows; i++)
            {
                max = Math.Max(max, input[i, 0]);
            }

            for (int i = 0; i < input.Rows; i++)
            {
                double exp = Math.Exp(input[i, 0] - max);
                result[i, 0] = exp;
                sum += exp;
            }

            for (int i = 0; i < input.Rows; i++)
            {
                result[i, 0] /= sum;
            }

            return result;
        }

        private double CalculateLoss(Matrix output, Matrix expected)
        {
            if (LossFunction == LossFunctionType.CrossEntropy)
            {
                double loss = 0;
                for (int i = 0; i < output.Rows; i++)
                {
                    loss -= expected[i, 0] * Math.Log(output[i, 0] + 1e-10);
                }
                return loss;
            }
            else
            {
                var diff = Matrix.Subtract(output, expected);
                double sumSquared = 0;
                for (int i = 0; i < diff.Rows; i++)
                {
                    sumSquared += diff[i, 0] * diff[i, 0];
                }
                return sumSquared / output.Rows;
            }
        }

        public void SaveToFile(string filePath)
        {
            string json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(filePath, json);
        }

        public static NeuralNetwork? LoadFromFile(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<NeuralNetwork>(json, options);
        }

        private void Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _random.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
}