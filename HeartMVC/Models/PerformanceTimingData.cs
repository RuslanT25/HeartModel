namespace HeartMVC.Models
{
    public class PerformanceTimingData
    {
        public long FastForestTrainingTime { get; set; }
        public long FastForestPredictionTime { get; set; }
        public long LogisticRegressionTrainingTime { get; set; }
        public long LogisticRegressionPredictionTime { get; set; }
        
        public string FasterTrainingModel => FastForestTrainingTime < LogisticRegressionTrainingTime ? "FastForest" : "LogisticRegression";
        public string FasterPredictionModel => FastForestPredictionTime < LogisticRegressionPredictionTime ? "FastForest" : "LogisticRegression";
    }
}