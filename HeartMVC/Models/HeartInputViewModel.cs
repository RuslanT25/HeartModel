using System.ComponentModel.DataAnnotations;

namespace HeartMVC.Models
{
    public class HeartInputViewModel
    {
        [Required]
        [Display(Name = "Yaş")]
        [Range(1, 120, ErrorMessage = "Yaş 1-120 arasında olmalıdır")]
        public int Age { get; set; }

        [Required]
        [Display(Name = "Cinsiyet")]
        public int Sex { get; set; } 

        [Required]
        [Display(Name = "Göğüs Ağrısı Tipi")]
        public int Cp { get; set; } 

        [Required]
        [Display(Name = "Dinlenme Kan Basıncı")]
        [Range(80, 250, ErrorMessage = "Kan basıncı 80-250 arasında olmalıdır")]
        public int Trestbps { get; set; }

        [Required]
        [Display(Name = "Kolesterol")]
        [Range(100, 600, ErrorMessage = "Kolesterol 100-600 arasında olmalıdır")]
        public int Chol { get; set; }

        [Required]
        [Display(Name = "Açlık Kan Şekeri > 120 mg/dl")]
        public int Fbs { get; set; } 

        [Required]
        [Display(Name = "Dinlenme EKG Sonuçları")]
        public int Restecg { get; set; } 

        [Required]
        [Display(Name = "Maksimum Kalp Atış Hızı")]
        [Range(60, 220, ErrorMessage = "Kalp atış hızı 60-220 arasında olmalıdır")]
        public int Thalach { get; set; }

        [Required]
        [Display(Name = "Egzersiz Kaynaklı Angina")]
        public int Exang { get; set; } 

        [Required]
        [Display(Name = "ST Depresyonu")]
        [Range(0, 10, ErrorMessage = "ST Depresyonu 0-10 arasında olmalıdır")]
        public float Oldpeak { get; set; }

        [Required]
        [Display(Name = "ST Segmenti Eğimi")]
        public int Slope { get; set; } 

        [Required]
        [Display(Name = "Büyük Damar Sayısı")]
        public int Ca { get; set; }

        [Required]
        [Display(Name = "Thalassemia")]
        public int Thal { get; set; }
    }
}