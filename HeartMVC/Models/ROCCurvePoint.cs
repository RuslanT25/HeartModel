namespace HeartMVC.Models
{
    public class ROCCurvePoint
    {
        public double Threshold { get; set; }
        public double TruePositiveRate { get; set; }
        public double FalsePositiveRate { get; set; }
    }
}