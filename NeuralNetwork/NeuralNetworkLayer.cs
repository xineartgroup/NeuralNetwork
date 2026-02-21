using System.Text.Json.Serialization;

namespace NeuralNetwork
{
    public class NeuralNetworkLayer
    {
        [JsonInclude]
        public int LayerSize { get; set; }

        [JsonInclude]
        public ActivationFunctionType ActivationFunction { get; set; }

        public NeuralNetworkLayer()
        {
            LayerSize = 0;
            ActivationFunction = ActivationFunctionType.Linear;
        }

        public NeuralNetworkLayer(int layerSize, ActivationFunctionType activationFunction)
        {
            LayerSize = layerSize;
            ActivationFunction = activationFunction;
        }
    }
}
