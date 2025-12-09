namespace HeartMVC.Models
{
    public class AdvancedAnalyticsViewModel
    {
        public List<FeatureImportanceData> FeatureImportance { get; set; } = new List<FeatureImportanceData>();
        public List<ROCCurvePoint> ROCCurveData { get; set; } = new List<ROCCurvePoint>(); // Logistic Regression ROC
        public List<ROCCurvePoint> FastForestROCCurveData { get; set; } = new List<ROCCurvePoint>(); // Random Forest ROC
        public List<CalibrationCurvePoint> LogisticCalibrationData { get; set; } = new List<CalibrationCurvePoint>(); // Logistic Regression Calibration
        public List<CalibrationCurvePoint> FastForestCalibrationData { get; set; } = new List<CalibrationCurvePoint>(); // Random Forest Calibration
        public PerformanceTimingData TimingData { get; set; } = new PerformanceTimingData();
        public DataStatistics DataStats { get; set; } = new DataStatistics();
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; } = DateTime.Now;
        public double OptimalThreshold { get; set; } // Logistic Regression optimal threshold
        public double AUCValue { get; set; } // Logistic Regression AUC
        public double FastForestOptimalThreshold { get; set; } // Random Forest optimal threshold
        public double FastForestAUCValue { get; set; } // Random Forest AUC
        public double LogisticCalibrationScore { get; set; } // Logistic Regression calibration quality score
        public double FastForestCalibrationScore { get; set; } // Random Forest calibration quality score
        public double LogisticLogLoss { get; set; } // Logistic Regression Log Loss score
        public double FastForestLogLoss { get; set; } // Random Forest Log Loss score
    }
}