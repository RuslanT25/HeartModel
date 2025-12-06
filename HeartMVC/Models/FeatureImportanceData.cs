namespace HeartMVC.Models
{
    public class FeatureImportanceData
    {
        public string FeatureName { get; set; } = string.Empty;
        public double ImportanceScore { get; set; }
        public double StandardDeviation { get; set; }
        public int Rank { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }
}