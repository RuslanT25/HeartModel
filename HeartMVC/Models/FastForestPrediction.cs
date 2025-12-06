using Microsoft.ML.Data;

namespace HeartMVC.Models
{
    public class FastForestPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        public float Score { get; set; }
        
        // FastForest için Label field'ı ROC curve hesaplaması için gerekli
        public bool Label { get; set; }
    }
}