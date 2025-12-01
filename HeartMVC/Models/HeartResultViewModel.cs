namespace HeartMVC.Models
{
    public class HeartResultViewModel
    {
        public HeartInputViewModel InputData { get; set; }
        
        // FastForest Classification 
        public bool HasHeartDisease { get; set; }
        
        // Logistic Regression  
        public float RiskProbability { get; set; }
        
        public string RiskLevel { get; set; }
        public string RiskColor { get; set; }
        public string Message { get; set; }
        public string ClassificationResult { get; set; }
    }
}