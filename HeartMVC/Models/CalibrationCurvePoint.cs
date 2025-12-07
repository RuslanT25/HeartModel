namespace HeartMVC.Models
{
    public class CalibrationCurvePoint
    {
        public double MeanPredictedProbability { get; set; } // Modelin tahmin ettiği ortalama probability
        public double ActualPositiveRate { get; set; } // Gerçek pozitif oranı
        public int BinCount { get; set; } // Bu bin'deki sample sayısı
        public double BinStart { get; set; } // Bin başlangıç değeri
        public double BinEnd { get; set; } // Bin bitiş değeri
    }
}