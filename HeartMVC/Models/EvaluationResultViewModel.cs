namespace HeartMVC.Models
{
    public class EvaluationResultViewModel
    {
        public ModelMetrics FastForestMetrics { get; set; } = new ModelMetrics();
        public ModelMetrics LogisticRegressionMetrics { get; set; } = new ModelMetrics();
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public DateTime EvaluationDate { get; set; } = DateTime.Now;
        public int TestDataSize { get; set; }
    }
}