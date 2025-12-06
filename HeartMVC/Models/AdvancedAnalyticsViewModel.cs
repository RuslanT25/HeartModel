namespace HeartMVC.Models
{
    public class AdvancedAnalyticsViewModel
    {
        public List<FeatureImportanceData> FeatureImportance { get; set; } = new List<FeatureImportanceData>();
        public List<ROCCurvePoint> ROCCurveData { get; set; } = new List<ROCCurvePoint>();
        public PerformanceTimingData TimingData { get; set; } = new PerformanceTimingData();
        public DataStatistics DataStats { get; set; } = new DataStatistics();
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; } = DateTime.Now;
        public double OptimalThreshold { get; set; }
        public double AUCValue { get; set; }
    }
}