using Microsoft.AspNetCore.Mvc;
using HeartMVC.Models;
using HeartMVC.Services;

namespace HeartMVC.Controllers
{
    public class HeartController : Controller
    {
        private readonly FastForestClassificationService _fastForestService;
        private readonly LogisticRegressionService _logisticService;
        private readonly ModelEvaluationService _evaluationService;

        public HeartController(
            FastForestClassificationService fastForestService,
            LogisticRegressionService logisticService,
            ModelEvaluationService evaluationService)
        {
            _fastForestService = fastForestService;
            _logisticService = logisticService;
            _evaluationService = evaluationService;
        }

        public async Task<IActionResult> Index()
        {
            var fastForestExists = await _fastForestService.EnsureModelExistsAsync();
            var logisticExists = await _logisticService.EnsureModelExistsAsync();
            
            if (!fastForestExists || !logisticExists)
            {
                ViewBag.Error = "Model dosyaları bulunamadı. Lütfen heart.csv dosyasının wwwroot/Data klasöründe olduğundan emin olun.";
            }

            return View(new HeartInputViewModel());
        }

        [HttpPost]
        public IActionResult Predict(HeartInputViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            try
            {
                var hasHeartDisease = _fastForestService.PredictClassification(model);
                var riskProbability = _logisticService.PredictProbability(model);

                var result = new HeartResultViewModel
                {
                    InputData = model,
                    HasHeartDisease = hasHeartDisease,
                    RiskProbability = riskProbability * 100, // Convert to percentage
                    ClassificationResult = hasHeartDisease ? "Risk Var" : "Risk Yok"
                };

                if (result.RiskProbability >= 70)
                {
                    result.RiskLevel = "Yüksek Risk";
                    result.RiskColor = "danger";
                    result.Message = "Kalp hastalığı riski yüksek. Lütfen bir kardiyolog ile görüşün.";
                }
                else if (result.RiskProbability >= 40)
                {
                    result.RiskLevel = "Orta Risk";
                    result.RiskColor = "warning";
                    result.Message = "Kalp hastalığı riski orta seviyede. Düzenli kontrol önerilir.";
                }
                else
                {
                    result.RiskLevel = "Düşük Risk";
                    result.RiskColor = "success";
                    result.Message = "Kalp hastalığı riski düşük. Sağlıklı yaşam tarzınızı sürdürün.";
                }

                return View("Result", result);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Tahmin sırasında hata oluştu: {ex.Message}";
                return View("Index", model);
            }
        }

        public async Task<IActionResult> Evaluation()
        {
            try
            {
                var evaluationResult = await _evaluationService.EvaluateModelsAsync();
                return View(evaluationResult);
            }
            catch (Exception ex)
            {
                var errorResult = new EvaluationResultViewModel
                {
                    HasError = true,
                    ErrorMessage = $"Model değerlendirme sırasında beklenmeyen bir hata oluştu: {ex.Message}"
                };
                return View(errorResult);
            }
        }
    }
}