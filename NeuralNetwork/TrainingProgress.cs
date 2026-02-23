namespace NeuralNetwork
{
    public class TrainingProgress
    {
        public int Epoch { get; set; }
        public int Batch { get; set; }
        public int TotalBatches { get; set; }
        public double CurrentLoss { get; set; }
        public double ProgressPercentage => (double)Batch / TotalBatches * 100;
    }
}
