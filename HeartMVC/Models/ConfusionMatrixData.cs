namespace HeartMVC.Models
{
    public class ConfusionMatrixData
    {
        public int TruePositive { get; set; }
        public int TrueNegative { get; set; }
        public int FalsePositive { get; set; }
        public int FalseNegative { get; set; }

        public int Total => TruePositive + TrueNegative + FalsePositive + FalseNegative;
        public int ActualPositive => TruePositive + FalseNegative;
        public int ActualNegative => TrueNegative + FalsePositive;
        public int PredictedPositive => TruePositive + FalsePositive;
        public int PredictedNegative => TrueNegative + FalseNegative;
    }
}