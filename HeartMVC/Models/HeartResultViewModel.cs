namespace HeartMVC.Models
{
    public class HeartResultViewModel
    {
        public HeartInputViewModel InputData { get; set; }
        
        // FastForest Classification Result
        public bool HasHeartDisease { get; set; }
        
        // Logistic Regression Probability Result
        public float RiskProbability { get; set; }
        
        // UI Display Properties
        public string RiskLevel { get; set; }
        public string RiskColor { get; set; }
        public string Message { get; set; }
        public string ClassificationResult { get; set; }
    }
}