using Microsoft.AspNetCore.Mvc;
using HeartMVC.Models;
using HeartMVC.Services;

namespace HeartMVC.Controllers
{
    public class HeartController : Controller
    {
        private readonly FastForestClassificationService _fastForestService;
        private readonly LogisticRegressionService _logisticService;

        public HeartController(
            FastForestClassificationService fastForestService,
            LogisticRegressionService logisticService)
        {
            _fastForestService = fastForestService;
            _logisticService = logisticService;
        }

        public async Task<IActionResult> Index()
        {
            // Ensure both models exist
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
                // Get predictions from both models
                var hasHeartDisease = _fastForestService.PredictClassification(model);
                var riskProbability = _logisticService.PredictProbability(model);

                var result = new HeartResultViewModel
                {
                    InputData = model,
                    HasHeartDisease = hasHeartDisease,
                    RiskProbability = riskProbability * 100, // Convert to percentage
                    ClassificationResult = hasHeartDisease ? "Risk Var" : "Risk Yok"
                };

                // Determine risk level and styling based on probability
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
    }
}