namespace HeartMVC.Models
{
    public class HeartResultViewModel
    {
        public HeartInputViewModel InputData { get; set; }
        public bool HasHeartDisease { get; set; }
        public float Score { get; set; }
        public float Probability { get; set; }
        public string RiskLevel { get; set; }
        public string RiskColor { get; set; }
        public string Message { get; set; }
    }
}