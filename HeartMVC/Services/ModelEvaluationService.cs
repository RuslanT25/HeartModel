using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using HeartMVC.Models;

namespace HeartMVC.Services
{
    public class ModelEvaluationService
    {
        private readonly MLContext _mlContext;
        private readonly string _dataPath;
        private readonly string _fastForestModelPath;
        private readonly string _logisticModelPath;

        public ModelEvaluationService(IWebHostEnvironment environment)
        {
            _mlContext = new MLContext(seed: 1);
            _dataPath = Path.Combine(environment.WebRootPath, "Data", "heart.csv");
            _fastForestModelPath = Path.Combine(environment.WebRootPath, "Data", "fastforest_model.zip");
            _logisticModelPath = Path.Combine(environment.WebRootPath, "Data", "logistic_model.zip");
        }

        public async Task<EvaluationResultViewModel> EvaluateModelsAsync()
        {
            var result = new EvaluationResultViewModel();

            try
            {
                if (!File.Exists(_dataPath))
                {
                    result.HasError = true;
                    result.ErrorMessage = "Dataset dosyası bulunamadı. Lütfen heart.csv dosyasının wwwroot/Data klasöründe olduğundan emin olun.";
                    return result;
                }

                if (!File.Exists(_fastForestModelPath) || !File.Exists(_logisticModelPath))
                {
                    result.HasError = true;
                    result.ErrorMessage = "Model dosyaları bulunamadı. Lütfen önce modellerin eğitildiğinden emin olun.";
                    return result;
                }

                var testData = await Task.Run(() => LoadAndSplitData());
                result.TestDataSize = (int)(testData.GetRowCount() ?? 0);

                result.FastForestMetrics = await Task.Run(() => EvaluateFastForest(testData));
                result.LogisticRegressionMetrics = await Task.Run(() => EvaluateLogisticRegression(testData));

                result.FastForestMetrics.ModelName = "Random Forest (FastForest)";
                result.LogisticRegressionMetrics.ModelName = "Logistic Regression";
            }
            catch (Exception ex)
            {
                result.HasError = true;
                result.ErrorMessage = $"Model değerlendirme sırasında hata oluştu: {ex.Message}";
            }

            return result;
        }

        private IDataView LoadAndSplitData()
        {
            IDataView dataView = _mlContext.Data.LoadFromTextFile<HeartData>(
                _dataPath,
                hasHeader: true,
                separatorChar: ',');

            var split = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
            return split.TestSet;
        }

        private ModelMetrics EvaluateFastForest(IDataView testData)
        {
            try
            {
                var model = _mlContext.Model.Load(_fastForestModelPath, out _);

                var predictions = model.Transform(testData);

                var metrics = _mlContext.BinaryClassification.EvaluateNonCalibrated(predictions);

                var confusionMatrix = ExtractConfusionMatrix(metrics.ConfusionMatrix);

                return new ModelMetrics
                {
                    Accuracy = metrics.Accuracy,
                    Precision = metrics.PositivePrecision,
                    Recall = metrics.PositiveRecall,
                    F1Score = metrics.F1Score,
                    AUC = metrics.AreaUnderRocCurve,
                    ConfusionMatrix = confusionMatrix
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"FastForest model evaluation failed: {ex.Message}", ex);
            }
        }

        private ModelMetrics EvaluateLogisticRegression(IDataView testData)
        {
            try
            {
                var model = _mlContext.Model.Load(_logisticModelPath, out _);

                var predictions = model.Transform(testData);

                var metrics = _mlContext.BinaryClassification.EvaluateNonCalibrated(predictions);

                var confusionMatrix = ExtractConfusionMatrix(metrics.ConfusionMatrix);

                return new ModelMetrics
                {
                    Accuracy = metrics.Accuracy,
                    Precision = metrics.PositivePrecision,
                    Recall = metrics.PositiveRecall,
                    F1Score = metrics.F1Score,
                    AUC = metrics.AreaUnderRocCurve,
                    ConfusionMatrix = confusionMatrix
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Logistic Regression model evaluation failed: {ex.Message}", ex);
            }
        }

        private ConfusionMatrixData ExtractConfusionMatrix(ConfusionMatrix matrix)
        {
            // [0,0] = True Negative, [0,1] = False Positive
            // [1,0] = False Negative, [1,1] = True Positive
            
            var counts = matrix.Counts;
            
            return new ConfusionMatrixData
            {
                TrueNegative = (int)counts[0][0],
                FalsePositive = (int)counts[0][1],
                FalseNegative = (int)counts[1][0],
                TruePositive = (int)counts[1][1]
            };
        }
    }
}