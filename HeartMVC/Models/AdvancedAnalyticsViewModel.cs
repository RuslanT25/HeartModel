namespace HeartMVC.Models
{
    public class AdvancedAnalyticsViewModel
    {
        public List<FeatureImportanceData> FeatureImportance { get; set; } = new List<FeatureImportanceData>();
        public List<ROCCurvePoint> ROCCurveData { get; set; } = new List<ROCCurvePoint>(); // Logistic Regression ROC
        public List<ROCCurvePoint> FastForestROCCurveData { get; set; } = new List<ROCCurvePoint>(); // Random Forest ROC
        public PerformanceTimingData TimingData { get; set; } = new PerformanceTimingData();
        public DataStatistics DataStats { get; set; } = new DataStatistics();
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; } = DateTime.Now;
        public double OptimalThreshold { get; set; } // Logistic Regression optimal threshold
        public double AUCValue { get; set; } // Logistic Regression AUC
        public double FastForestOptimalThreshold { get; set; } // Random Forest optimal threshold
        public double FastForestAUCValue { get; set; } // Random Forest AUC
    }
}