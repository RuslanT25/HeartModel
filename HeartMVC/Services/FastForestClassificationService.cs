using Microsoft.ML;
using Microsoft.ML.Trainers.FastTree;
using HeartMVC.Models;

namespace HeartMVC.Services
{
    public class FastForestClassificationService
    {
        private readonly MLContext _mlContext;
        private readonly string _modelPath;
        private readonly string _dataPath;
        private ITransformer? _model;

        public FastForestClassificationService(IWebHostEnvironment environment)
        {
            _mlContext = new MLContext(seed: 1);
            _modelPath = Path.Combine(environment.WebRootPath, "Data", "fastforest_model.zip");
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
                IDataView dataView = _mlContext.Data.LoadFromTextFile<HeartData>(
                    _dataPath,
                    hasHeader: true,
                    separatorChar: ',');

                var split = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
                var trainSet = split.TrainSet;

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
                    .Append(_mlContext.BinaryClassification.Trainers.FastForest(
                        new FastForestBinaryTrainer.Options
                        {
                            NumberOfTrees = 200,
                            NumberOfLeaves = 20,
                            LabelColumnName = "Label",
                            FeatureColumnName = "Features",
                        }));

                var model = pipeline.Fit(trainSet);

                _mlContext.Model.Save(model, trainSet.Schema, _modelPath);
            });
        }

        public bool PredictClassification(HeartInputViewModel input)
        {
            if (_model == null)
            {
                _model = _mlContext.Model.Load(_modelPath, out _);
            }

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

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<HeartData, HeartPrediction>(_model);

            var prediction = predictionEngine.Predict(heartData);
            return prediction.Prediction;
        }
    }
}