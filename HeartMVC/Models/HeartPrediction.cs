using Microsoft.ML.Data;

namespace HeartMVC.Models
{
    public class HeartPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }

        public float Score { get; set; }
        
        public float Probability { get; set; }
        
        // For ROC curve generation
        public bool Label { get; set; }
    }
}