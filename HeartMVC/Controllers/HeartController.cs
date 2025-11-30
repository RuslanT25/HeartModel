using Microsoft.AspNetCore.Mvc;
using HeartMVC.Models;
using HeartMVC.Services;

namespace HeartMVC.Controllers
{
    public class HeartController : Controller
    {
        private readonly HeartPredictionService _predictionService;

        public HeartController(HeartPredictionService predictionService)
        {
            _predictionService = predictionService;
        }

        public async Task<IActionResult> Index()
        {
            // Ensure model exists
            var modelExists = await _predictionService.EnsureModelExistsAsync();
            if (!modelExists)
            {
                ViewBag.Error = "Model dosyası bulunamadı. Lütfen heart.csv dosyasının wwwroot/Data klasöründe olduğundan emin olun.";
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
                var prediction = _predictionService.PredictHeartDisease(model);

                var result = new HeartResultViewModel
                {
                    InputData = model,
                    HasHeartDisease = prediction.Prediction,
                    Score = prediction.Score,
                    Probability = prediction.Probability * 100, // Convert to percentage
                };

                // Determine risk level and styling
                if (result.Probability >= 70)
                {
                    result.RiskLevel = "Yüksek Risk";
                    result.RiskColor = "danger";
                    result.Message = "Kalp hastalığı riski yüksek. Lütfen bir kardiyolog ile görüşün.";
                }
                else if (result.Probability >= 40)
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