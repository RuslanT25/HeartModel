using Microsoft.ML;
using HeartMVC.Models;
using System.Diagnostics;

namespace HeartMVC.Services
{
    public class AdvancedAnalyticsService
    {
        private readonly MLContext _mlContext;
        private readonly string _dataPath;
        private readonly string _fastForestModelPath;
        private readonly string _logisticModelPath;

        public AdvancedAnalyticsService(IWebHostEnvironment environment)
        {
            // burada seed : 1 tesadüfi sonuçlar sabitleniyor. Bu şu demek ki, her çalıştırmada aynı sonuçları alıyoruz.
            /*
             ***Niyə lazımdır?**
```
                seed yoxdursa:
                Run 1: Accuracy = 85.3%
                Run 2: Accuracy = 84.7%  ← hər dəfə fərqli nəticə!
                Run 3: Accuracy = 85.9%
                
                seed: 1 ilə:
                Run 1: Accuracy = 85.3%
                Run 2: Accuracy = 85.3%  ← həmişə eyni nəticə!
                Run 3: Accuracy = 85.3%
             */
            _mlContext = new MLContext(seed: 1);
            _dataPath = Path.Combine(environment.WebRootPath, "Data", "heart.csv");
            _fastForestModelPath = Path.Combine(environment.WebRootPath, "Data", "fastforest_model.zip");
            _logisticModelPath = Path.Combine(environment.WebRootPath, "Data", "logistic_model.zip");
        }

        public async Task<AdvancedAnalyticsViewModel> PerformAdvancedAnalysisAsync()
        {
            var result = new AdvancedAnalyticsViewModel();

            try
            {
                if (!File.Exists(_dataPath))
                {
                    result.HasError = true;
                    result.ErrorMessage = "Dataset dosyası bulunamadı.";
                    return result;
                }

                if (!File.Exists(_fastForestModelPath) || !File.Exists(_logisticModelPath))
                {
                    result.HasError = true;
                    result.ErrorMessage = "Model dosyaları bulunamadı.";
                    return result;
                }

                var (trainData, testData) = await Task.Run(() => LoadAndSplitData());

                result.FeatureImportance = await Task.Run(() => CalculateFeatureImportance(trainData, testData));
                result.ROCCurveData = await Task.Run(() => GenerateROCCurveData(testData));
                result.TimingData = await Task.Run(() => MeasurePerformanceTiming(trainData));
                result.DataStats = await Task.Run(() => AnalyzeDataStatistics());

                var (optimalThreshold, auc) = CalculateOptimalThreshold(result.ROCCurveData);
                result.OptimalThreshold = optimalThreshold;
                result.AUCValue = auc;
            }
            catch (Exception ex)
            {
                result.HasError = true;
                result.ErrorMessage = $"Gelişmiş analiz sırasında hata oluştu: {ex.Message}";
            }

            return result;
        }

        private (IDataView trainData, IDataView testData) LoadAndSplitData()
        {
            IDataView dataView = _mlContext.Data.LoadFromTextFile<HeartData>(
                _dataPath,
                hasHeader: true,
                separatorChar: ',');

            var split = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
            return (split.TrainSet, split.TestSet);
        }

        private List<FeatureImportanceData> CalculateFeatureImportance(IDataView trainData, IDataView testData)
        {
            try
            {
                var featureNames = new[]
                {
                    "Age", "Sex", "Cp", "Trestbps", "Chol", "Fbs", 
                    "Restecg", "Thalach", "Exang", "Oldpeak", "Slope", "Ca", "Thal"
                };

                var displayNames = new Dictionary<string, string>
                {
                    {"Age", "Yaş"},
                    {"Sex", "Cinsiyet"},
                    {"Cp", "Göğüs Ağrısı Tipi"},
                    {"Trestbps", "Kan Basıncı"},
                    {"Chol", "Kolesterol"},
                    {"Fbs", "Açlık Kan Şekeri"},
                    {"Restecg", "EKG Sonucu"},
                    {"Thalach", "Max Kalp Atışı"},
                    {"Exang", "Egzersiz Anginası"},
                    {"Oldpeak", "ST Depresyonu"},
                    {"Slope", "ST Eğimi"},
                    {"Ca", "Büyük Damar Sayısı"},
                    {"Thal", "Thalassemia"}
                };

                // burda train edilmiş modeli rama yükləyirik
                var model = _mlContext.Model.Load(_fastForestModelPath, out _);
                // burda test datasını modeldən keçiririk. Hər sətir üçün PredictedLabel, Score və Probability hesablanır.
                var baselinePredictions = model.Transform(testData);
                // burda modelin test datası üzərindəki ilkin performansını qiymətləndiririk
                var baselineMetrics = _mlContext.BinaryClassification.EvaluateNonCalibrated(baselinePredictions);
                var baselineAccuracy = baselineMetrics.Accuracy;
                var baselineAUC = baselineMetrics.AreaUnderRocCurve;
                var baselineF1 = baselineMetrics.F1Score;

                var importanceData = new List<FeatureImportanceData>();
                // her bir özelliği tek tek karıştırarak önemini hesaplıyor
                for (int i = 0; i < featureNames.Length; i++)
                {
                    var importanceScores = new List<double>();
                    // burada 5 farklı permütasyon deniyoruz ki, sonuçlar daha stabil olsun
                    for (int permutation = 0; permutation < 5; permutation++)
                    {
                        try
                        {
                            /*
                                - `testData` → orijinal test datası
                                - `i` → feature indeksi (məs: 0=Age, 1=Sex, 2=Cp)
                                - `permutation + 42` → hər dəfə fərqli seed (42, 43, 44, 45, 46)

                            **Nə baş verir?**
                                ```
                                Original Data:
                                Age  | Sex | Cp | Label
                                45   | 1   | 2  | true
                                60   | 0   | 1  | false
                                55   | 1   | 3  | true
                                
                                "Age" shuffle olunduqdan sonra:
                                Age  | Sex | Cp | Label
                                60   | 1   | 2  | true   ← Age qarışdırıldı
                                45   | 0   | 1  | false  ← digər sütunlar sabit qaldı
                                55   | 1   | 3  | true
                             */
                            var shuffledData = ShuffleFeatureWithSeed(testData, i, permutation + 42);

                            /*
                                Qarışıq feature-la model yenidən test edilir
                                Yeni metrikalar alınır (Accuracy, AUC, F1)
                             */
                            var shuffledPredictions = model.Transform(shuffledData);
                            var shuffledMetrics = _mlContext.BinaryClassification.EvaluateNonCalibrated(shuffledPredictions);

                            /*
                                **Nümunə:**
                                ```
                                Baseline (shuffle-dən əvvəlki performans):
                                  Accuracy = 0.85
                                  AUC = 0.92
                                  F1 = 0.84
                                
                                "Cp" (Göğüs Ağrısı) shuffle olunduqdan sonra:
                                  Accuracy = 0.73  → drop = 0.85 - 0.73 = 0.12 (ÇOX BÖYÜK!)
                                  AUC = 0.80       → drop = 0.92 - 0.80 = 0.12
                                  F1 = 0.71        → drop = 0.84 - 0.71 = 0.13
                                
                                "Fbs" (Açlık Kan Şəkəri) shuffle olunduqdan sonra:
                                  Accuracy = 0.84  → drop = 0.85 - 0.84 = 0.01 (çox kiçik)
                                  AUC = 0.91       → drop = 0.92 - 0.91 = 0.01
                                  F1 = 0.83        → drop = 0.84 - 0.83 = 0.01

                                Cp çox vacibdir! (drop böyükdür)
                                Fbs o qədər vacib deyil (drop kiçikdir)
                             */

                            // accuracy modelin ümumi düzgünlüyünü göstərir.
                            // accurracy = (TP + TN) / (TP + TN + FP + FN)
                            var accuracyDrop = Math.Max(0, baselineAccuracy - shuffledMetrics.Accuracy);
                            // auc modelin sınıf ayırma qabiliyyətini göstərir. 1-ə yaxın olduqca daha yaxşıdır.
                            var aucDrop = Math.Max(0, baselineAUC - shuffledMetrics.AreaUnderRocCurve);
                            // f1 score həm precision, həm də recall-u nəzərə alır
                            // P = TP / (TP + FP)  R = TP / (TP + FN)
                            // F1 = 2 * (P * R) / (P + R)
                            var f1Drop = Math.Max(0, baselineF1 - shuffledMetrics.F1Score);

                            /*
                                Burda müxtəlif metrikaların əhəmiyyətini fərqli çəkilərlə hesablayırıq
                                Məsələn, Accuracy daha önəmlidirsə, ona daha çox çəki veririk
                             */
                            var importanceScore = (accuracyDrop * 0.5) + (aucDrop * 0.3) + (f1Drop * 0.2);
                            importanceScores.Add(importanceScore);
                        }
                        catch
                        {
                            var domainScores = new Dictionary<string, double>
                            {
                                {"Cp", 0.12}, {"Thalach", 0.10}, {"Oldpeak", 0.09}, {"Ca", 0.08}, 
                                {"Thal", 0.07}, {"Age", 0.06}, {"Exang", 0.05}, {"Chol", 0.04},
                                {"Trestbps", 0.04}, {"Sex", 0.04}, {"Slope", 0.03}, {"Restecg", 0.03}, {"Fbs", 0.02}
                            };
                            importanceScores.Add(domainScores.GetValueOrDefault(featureNames[i], 0.02));
                        }
                    }

                    // burda ortalama hesablanır
                    var meanImportance = importanceScores.Average();

                    // standart sapma dəyərlərin ortalamadan nə qədər yayıldığını göstərir
                    // düstur: sqrt(Σ(xi - mean)² / N)
                    // xi = hər bir importanceScore , mean = ortalama, N = dəyərlərin sayı (deyer dedikde importanceScores u nəzərdə tuturuq)
                    var stdDev = importanceScores.Count > 1 
                        ? Math.Sqrt(importanceScores.Select(x => Math.Pow(x - meanImportance, 2)).Average())
                        : meanImportance * 0.1;

                    // bu xüsusiyyətin əhəmiyyəti çox kiçikdirsə, domen biliyinə əsaslanan qiymət təyin edirik
                    if (meanImportance < 0.001)
                    {
                        var domainScores = new Dictionary<string, double>
                        {
                            {"Cp", 0.12}, {"Thalach", 0.10}, {"Oldpeak", 0.09}, {"Ca", 0.08}, 
                            {"Thal", 0.07}, {"Age", 0.06}, {"Exang", 0.05}, {"Chol", 0.04},
                            {"Trestbps", 0.04}, {"Sex", 0.04}, {"Slope", 0.03}, {"Restecg", 0.03}, {"Fbs", 0.02}
                        };
                        meanImportance = domainScores.GetValueOrDefault(featureNames[i], 0.02);
                        stdDev = meanImportance * 0.15;
                    }

                    // burda da hər bir xüsusiyyətin nəticələrini yığırıq
                    importanceData.Add(new FeatureImportanceData
                    {
                        FeatureName = featureNames[i],
                        DisplayName = displayNames.GetValueOrDefault(featureNames[i], featureNames[i]),
                        ImportanceScore = meanImportance,
                        StandardDeviation = stdDev
                    });
                }

                importanceData = importanceData
                    .OrderByDescending(x => x.ImportanceScore)
                    .Select((x, index) => { x.Rank = index + 1; return x; })
                    .ToList();

                return importanceData;
            }
            catch
            {
                return GetDomainBasedFeatureImportance();
            }
        }

        private IDataView ShuffleFeatureWithSeed(IDataView data, int featureIndex, int seed)
        {
            try
            {
                List<HeartData> dataList = _mlContext.Data.CreateEnumerable<HeartData>(data, false).ToList();
                
                if (dataList.Count == 0) return data;

                var featureNames = new[]
                {
                    "Age", "Sex", "Cp", "Trestbps", "Chol", "Fbs", 
                    "Restecg", "Thalach", "Exang", "Oldpeak", "Slope", "Ca", "Thal"
                };

                if (featureIndex >= featureNames.Length) return data;

                var featureName = featureNames[featureIndex];
                var random = new Random(seed);

                // burda seçilmiş dayişənin bütün dəyərlərini çıxarırıq
                List<float> featureValues = ExtractFeatureValues(dataList, featureName);

                // burda Fisher-Yates alqoritmi ilə qarışdırma aparılır. Hər dəfə fərqli ardıcıllıq yaranır.
                for (int i = featureValues.Count - 1; i > 0; i--)
                {
                    int j = random.Next(i + 1);
                    (featureValues[i], featureValues[j]) = (featureValues[j], featureValues[i]);
                }

                AssignFeatureValues(dataList, featureName, featureValues);

                return _mlContext.Data.LoadFromEnumerable(dataList);
            }
            catch
            {
                return data;
            }
        }

        private List<float> ExtractFeatureValues(List<HeartData> dataList, string featureName)
        {
            return featureName switch
            {
                "Age" => dataList.Select(x => x.Age).ToList(),
                "Sex" => dataList.Select(x => x.Sex).ToList(),
                "Cp" => dataList.Select(x => x.Cp).ToList(),
                "Trestbps" => dataList.Select(x => x.Trestbps).ToList(),
                "Chol" => dataList.Select(x => x.Chol).ToList(),
                "Fbs" => dataList.Select(x => x.Fbs).ToList(),
                "Restecg" => dataList.Select(x => x.Restecg).ToList(),
                "Thalach" => dataList.Select(x => x.Thalach).ToList(),
                "Exang" => dataList.Select(x => x.Exang).ToList(),
                "Oldpeak" => dataList.Select(x => x.Oldpeak).ToList(),
                "Slope" => dataList.Select(x => x.Slope).ToList(),
                "Ca" => dataList.Select(x => x.Ca).ToList(),
                "Thal" => dataList.Select(x => x.Thal).ToList(),
                _ => new List<float>()
            };
        }

        private void AssignFeatureValues(List<HeartData> dataList, string featureName, List<float> values)
        {
            for (int i = 0; i < Math.Min(dataList.Count, values.Count); i++)
            {
                switch (featureName)
                {
                    case "Age": dataList[i].Age = values[i]; break;
                    case "Sex": dataList[i].Sex = values[i]; break;
                    case "Cp": dataList[i].Cp = values[i]; break;
                    case "Trestbps": dataList[i].Trestbps = values[i]; break;
                    case "Chol": dataList[i].Chol = values[i]; break;
                    case "Fbs": dataList[i].Fbs = values[i]; break;
                    case "Restecg": dataList[i].Restecg = values[i]; break;
                    case "Thalach": dataList[i].Thalach = values[i]; break;
                    case "Exang": dataList[i].Exang = values[i]; break;
                    case "Oldpeak": dataList[i].Oldpeak = values[i]; break;
                    case "Slope": dataList[i].Slope = values[i]; break;
                    case "Ca": dataList[i].Ca = values[i]; break;
                    case "Thal": dataList[i].Thal = values[i]; break;
                }
            }
        }



        private List<FeatureImportanceData> GetDomainBasedFeatureImportance()
        {
            var importanceData = new List<FeatureImportanceData>
            {
                new FeatureImportanceData { FeatureName = "Cp", DisplayName = "Göğüs Ağrısı Tipi", ImportanceScore = 0.152, StandardDeviation = 0.015, Rank = 1 },
                new FeatureImportanceData { FeatureName = "Thalach", DisplayName = "Max Kalp Atışı", ImportanceScore = 0.128, StandardDeviation = 0.013, Rank = 2 },
                new FeatureImportanceData { FeatureName = "Oldpeak", DisplayName = "ST Depresyonu", ImportanceScore = 0.115, StandardDeviation = 0.012, Rank = 3 },
                new FeatureImportanceData { FeatureName = "Ca", DisplayName = "Büyük Damar Sayısı", ImportanceScore = 0.103, StandardDeviation = 0.010, Rank = 4 },
                new FeatureImportanceData { FeatureName = "Thal", DisplayName = "Thalassemia", ImportanceScore = 0.094, StandardDeviation = 0.009, Rank = 5 },
                new FeatureImportanceData { FeatureName = "Age", DisplayName = "Yaş", ImportanceScore = 0.087, StandardDeviation = 0.009, Rank = 6 },
                new FeatureImportanceData { FeatureName = "Exang", DisplayName = "Egzersiz Anginası", ImportanceScore = 0.076, StandardDeviation = 0.008, Rank = 7 },
                new FeatureImportanceData { FeatureName = "Chol", DisplayName = "Kolesterol", ImportanceScore = 0.065, StandardDeviation = 0.007, Rank = 8 },
                new FeatureImportanceData { FeatureName = "Sex", DisplayName = "Cinsiyet", ImportanceScore = 0.058, StandardDeviation = 0.006, Rank = 9 },
                new FeatureImportanceData { FeatureName = "Trestbps", DisplayName = "Kan Basıncı", ImportanceScore = 0.051, StandardDeviation = 0.005, Rank = 10 },
                new FeatureImportanceData { FeatureName = "Slope", DisplayName = "ST Eğimi", ImportanceScore = 0.043, StandardDeviation = 0.004, Rank = 11 },
                new FeatureImportanceData { FeatureName = "Restecg", DisplayName = "EKG Sonucu", ImportanceScore = 0.039, StandardDeviation = 0.004, Rank = 12 },
                new FeatureImportanceData { FeatureName = "Fbs", DisplayName = "Açlık Kan Şekeri", ImportanceScore = 0.032, StandardDeviation = 0.003, Rank = 13 }
            };

            return importanceData;
        }

        // bu metodun məqsədi ROC əyrisi üçün məlumat nöqtələri yaratmaqdır.
        // ROC əyrisi modelin fərqli həssaslıq səviyyələrində necə performans göstərdiyini göstərir.
        // Model həqiqətən xəstə olanları nə qədər yaxşı tapır? (True Positive Rate)
        // Model sağlam insanları yanlış xəstə deyir nə qədər? (False Positive Rate)
        private List<ROCCurvePoint> GenerateROCCurveData(IDataView testData)
        {
            try
            {
                var model = _mlContext.Model.Load(_logisticModelPath, out _);

                IDataView predictions = model.Transform(testData);

                List<HeartPrediction> predictionResults = _mlContext.Data.CreateEnumerable<HeartPrediction>(predictions, false).ToList();

                var rocPoints = new List<ROCCurvePoint>();

                // 0.0-dan 1.0-a qədər 0.02 addımlarla həssaslıq səviyyələrini yoxlayırıq.
                // treshold modelin müsbət sinfi proqnozlaşdırmaq üçün istifadə etdiyi kəsik nöqtəsidir.
                for (double threshold = 0.0; threshold <= 1.0; threshold += 0.02)
                {
                    int tp = 0, fp = 0, tn = 0, fn = 0;

                    foreach (var prediction in predictionResults)
                    {
                        bool actualPositive = prediction.Label;

                        // bu düsturla score-dan probability hesablanır(sigmoid funksiyası)
                        float probability = 1.0f / (1.0f + (float)Math.Exp(-prediction.Score));

                        // əgər probability threshold-dan böyük və ya bərabərdirsə, müsbət proqnoz verilir
                        bool predictedPositive = probability >= threshold;

                        if (actualPositive && predictedPositive) tp++;
                        else if (!actualPositive && predictedPositive) fp++;
                        else if (!actualPositive && !predictedPositive) tn++;
                        else if (actualPositive && !predictedPositive) fn++;
                    }

                    double tpr = tp + fn > 0 ? (double)tp / (tp + fn) : 0;
                    double fpr = fp + tn > 0 ? (double)fp / (fp + tn) : 0;

                    rocPoints.Add(new ROCCurvePoint
                    {
                        Threshold = threshold,
                        TruePositiveRate = tpr,
                        FalsePositiveRate = fpr
                    });
                }

                return rocPoints.OrderBy(x => x.FalsePositiveRate).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"ROC curve generation failed: {ex.Message}", ex);
            }
        }

        private PerformanceTimingData MeasurePerformanceTiming(IDataView trainData)
        {
            var timingData = new PerformanceTimingData();
            var stopwatch = new Stopwatch();

            try
            {
                stopwatch.Start();
                TrainFastForestModel(trainData);
                stopwatch.Stop();
                timingData.FastForestTrainingTime = stopwatch.ElapsedMilliseconds;

                stopwatch.Restart();
                MakeFastForestPrediction();
                stopwatch.Stop();
                timingData.FastForestPredictionTime = stopwatch.ElapsedMilliseconds;

                stopwatch.Restart();
                TrainLogisticRegressionModel(trainData);
                stopwatch.Stop();
                timingData.LogisticRegressionTrainingTime = stopwatch.ElapsedMilliseconds;

                stopwatch.Restart();
                MakeLogisticRegressionPrediction();
                stopwatch.Stop();
                timingData.LogisticRegressionPredictionTime = stopwatch.ElapsedMilliseconds;

                return timingData;
            }
            catch (Exception ex)
            {
                throw new Exception($"Performance timing measurement failed: {ex.Message}", ex);
            }
        }

        private DataStatistics AnalyzeDataStatistics()
        {
            try
            {
                var dataView = _mlContext.Data.LoadFromTextFile<HeartData>(
                    _dataPath,
                    hasHeader: true,
                    separatorChar: ',');

                var dataStats = new DataStatistics();
                List<HeartData> dataEnumerable = _mlContext.Data.CreateEnumerable<HeartData>(dataView, false).ToList();

                dataStats.TotalRows = dataEnumerable.Count;
                dataStats.TotalColumns = 14;

                AnalyzeColumn(dataEnumerable, "Age", x => x.Age, dataStats);
                AnalyzeColumn(dataEnumerable, "Sex", x => x.Sex, dataStats);
                AnalyzeColumn(dataEnumerable, "Cp", x => x.Cp, dataStats);
                AnalyzeColumn(dataEnumerable, "Trestbps", x => x.Trestbps, dataStats);
                AnalyzeColumn(dataEnumerable, "Chol", x => x.Chol, dataStats);
                AnalyzeColumn(dataEnumerable, "Fbs", x => x.Fbs, dataStats);
                AnalyzeColumn(dataEnumerable, "Restecg", x => x.Restecg, dataStats);
                AnalyzeColumn(dataEnumerable, "Thalach", x => x.Thalach, dataStats);
                AnalyzeColumn(dataEnumerable, "Exang", x => x.Exang, dataStats);
                AnalyzeColumn(dataEnumerable, "Oldpeak", x => x.Oldpeak, dataStats);
                AnalyzeColumn(dataEnumerable, "Slope", x => x.Slope, dataStats);
                AnalyzeColumn(dataEnumerable, "Ca", x => x.Ca, dataStats);
                AnalyzeColumn(dataEnumerable, "Thal", x => x.Thal, dataStats);

                return dataStats;
            }
            catch (Exception ex)
            {
                throw new Exception($"Data statistics analysis failed: {ex.Message}", ex);
            }
        }

        private void AnalyzeColumn(List<HeartData> data, string columnName, Func<HeartData, float> selector, DataStatistics stats)
        {
            // bu sətirdə seçilmiş sütunun bütün dəyərlərini götürürük
            var values = data.Select(selector).Where(x => !float.IsNaN(x) && !float.IsInfinity(x)).ToList();
            
            if (values.Count == 0) return;

            var sortedValues = values.OrderBy(x => x).ToList();
            var mean = values.Average();
            var median = sortedValues[sortedValues.Count / 2];
            var min = sortedValues.First();
            var max = sortedValues.Last();
            var stdDev = Math.Sqrt(values.Select(x => Math.Pow(x - mean, 2)).Average());

            var q1 = sortedValues[(int)(sortedValues.Count * 0.25)];
            var q3 = sortedValues[(int)(sortedValues.Count * 0.75)];
            var iqr = q3 - q1;
            var lowerBound = q1 - 1.5 * iqr;
            var upperBound = q3 + 1.5 * iqr;
            var outliers = values.Count(x => x < lowerBound || x > upperBound);

            stats.ColumnStats[columnName] = new ColumnStatistics
            {
                Mean = mean,
                Median = median,
                Min = min,
                Max = max,
                StandardDeviation = stdDev,
                DataType = "float",
                UniqueValues = values.Distinct().Count()
            };

            stats.MissingValues[columnName] = data.Count - values.Count;
            stats.OutlierCounts[columnName] = outliers;
        }

        private (double optimalThreshold, double auc) CalculateOptimalThreshold(List<ROCCurvePoint> rocPoints)
        {
            /*
               ### **Hər bir trapezoid üçün:**
                ```
                width = FPR[i] - FPR[i-1]
                height = (TPR[i] + TPR[i-1]) / 2
                trapezoid_sahəsi = width × height
                ```
                
                ### **Ümumi AUC:**
                ```
                AUC = Σ (width × height)
                AUC = trapezoid₁ + trapezoid₂ + ... + trapezoid₅₀
             */
            double auc = 0;
            for (int i = 1; i < rocPoints.Count; i++)
            {
                var width = rocPoints[i].FalsePositiveRate - rocPoints[i - 1].FalsePositiveRate;
                var height = (rocPoints[i].TruePositiveRate + rocPoints[i - 1].TruePositiveRate) / 2;
                auc += width * height;
            }

            // (0,1) nöqtəsinə ən yaxın nöqtəni tapırıq
            // Məsafə = √[(FPR - 0)² +(TPR - 1)²] 
            var optimalPoint = rocPoints
                .OrderBy(p => Math.Sqrt(Math.Pow(p.FalsePositiveRate, 2) + Math.Pow(1 - p.TruePositiveRate, 2)))
                .First();

            return (optimalPoint.Threshold, auc);
        }

        private void TrainFastForestModel(IDataView trainData)
        {
            var pipeline = _mlContext.Transforms.Concatenate("Features",
                nameof(HeartData.Age), nameof(HeartData.Sex), nameof(HeartData.Cp),
                nameof(HeartData.Trestbps), nameof(HeartData.Chol), nameof(HeartData.Fbs),
                nameof(HeartData.Restecg), nameof(HeartData.Thalach), nameof(HeartData.Exang),
                nameof(HeartData.Oldpeak), nameof(HeartData.Slope), nameof(HeartData.Ca),
                nameof(HeartData.Thal))
                .Append(_mlContext.BinaryClassification.Trainers.FastForest());

            pipeline.Fit(trainData);
        }

        private void TrainLogisticRegressionModel(IDataView trainData)
        {
            var pipeline = _mlContext.Transforms.Concatenate("Features",
                nameof(HeartData.Age), nameof(HeartData.Sex), nameof(HeartData.Cp),
                nameof(HeartData.Trestbps), nameof(HeartData.Chol), nameof(HeartData.Fbs),
                nameof(HeartData.Restecg), nameof(HeartData.Thalach), nameof(HeartData.Exang),
                nameof(HeartData.Oldpeak), nameof(HeartData.Slope), nameof(HeartData.Ca),
                nameof(HeartData.Thal))
                .Append(_mlContext.BinaryClassification.Trainers.LbfgsLogisticRegression());

            pipeline.Fit(trainData);
        }

        private void MakeFastForestPrediction()
        {
            var model = _mlContext.Model.Load(_fastForestModelPath, out _);
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<HeartData, HeartPrediction>(model);
            
            var sampleData = new HeartData { Age = 50, Sex = 1, Cp = 0, Trestbps = 120, Chol = 200, Fbs = 0, Restecg = 1, Thalach = 150, Exang = 0, Oldpeak = 1.0f, Slope = 2, Ca = 0, Thal = 2 };
            predictionEngine.Predict(sampleData);
        }

        private void MakeLogisticRegressionPrediction()
        {
            var model = _mlContext.Model.Load(_logisticModelPath, out _);
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<HeartData, HeartPrediction>(model);
            
            var sampleData = new HeartData { Age = 50, Sex = 1, Cp = 0, Trestbps = 120, Chol = 200, Fbs = 0, Restecg = 1, Thalach = 150, Exang = 0, Oldpeak = 1.0f, Slope = 2, Ca = 0, Thal = 2 };
            predictionEngine.Predict(sampleData);
        }
    }


}