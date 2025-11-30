using Microsoft.ML;
using HeartMVC.Models;

namespace HeartMVC.Services
{
    public class LogisticRegressionService
    {
        private readonly MLContext _mlContext;
        private readonly string _modelPath;
        private readonly string _dataPath;
        private ITransformer? _model;

        public LogisticRegressionService(IWebHostEnvironment environment)
        {
            _mlContext = new MLContext(seed: 1);
            _modelPath = Path.Combine(environment.WebRootPath, "Data", "logistic_model.zip");
            _dataPath = Path.Combine(environment.WebRootPath, "Data", "heart.csv");
        }

        public async Task<bool> EnsureModelExistsAsync()
        {
            if (!File.Exists(_modelPath))
            {
                if (!File.Exists(_dataPath))
                {
                    return false;
                }
                await TrainModelAsync();
            }
            return true;
        }

        private async Task TrainModelAsync()
        {
            await Task.Run(() =>
            {
                // Load CSV
                IDataView dataView = _mlContext.Data.LoadFromTextFile<HeartData>(
                    _dataPath,
                    hasHeader: true,
                    separatorChar: ',');

                // Split into train/test
                var split = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
                var trainSet = split.TrainSet;

                // Build pipeline for Logistic Regression
                var pipeline = _mlContext.Transforms.Concatenate(
                        "Features",
                        nameof(HeartData.Age),
                        nameof(HeartData.Sex),
                        nameof(HeartData.Cp),
                        nameof(HeartData.Trestbps),
                        nameof(HeartData.Chol),
                        nameof(HeartData.Fbs),
                        nameof(HeartData.Restecg),
                        nameof(HeartData.Thalach),
                        nameof(HeartData.Exang),
                        nameof(HeartData.Oldpeak),
                        nameof(HeartData.Slope),
                        nameof(HeartData.Ca),
                        nameof(HeartData.Thal))
                    .Append(_mlContext.BinaryClassification.Trainers.LbfgsLogisticRegression(
                        labelColumnName: "Label",
                        featureColumnName: "Features"));

                // Train model
                var model = pipeline.Fit(trainSet);

                // Save model
                _mlContext.Model.Save(model, trainSet.Schema, _modelPath);
            });
        }

        public float PredictProbability(HeartInputViewModel input)
        {
            // Load model if not loaded
            if (_model == null)
            {
                _model = _mlContext.Model.Load(_modelPath, out _);
            }

            // Convert input to HeartData
            var heartData = new HeartData
            {
                Age = input.Age,
                Sex = input.Sex,
                Cp = input.Cp,
                Trestbps = input.Trestbps,
                Chol = input.Chol,
                Fbs = input.Fbs,
                Restecg = input.Restecg,
                Thalach = input.Thalach,
                Exang = input.Exang,
                Oldpeak = input.Oldpeak,
                Slope = input.Slope,
                Ca = input.Ca,
                Thal = input.Thal
            };

            // Create prediction engine
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<HeartData, HeartPrediction>(_model);

            // Make prediction and calculate probability
            var prediction = predictionEngine.Predict(heartData);
            
            // Convert score to probability using sigmoid function
            float probability = 1.0f / (1.0f + (float)Math.Exp(-prediction.Score));
            
            return probability;
        }
    }
}