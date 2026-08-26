using System.ComponentModel.DataAnnotations;

namespace chefeia.Models
{
    public class AdminPlanosViewModel
    {
        public int FreeId { get; set; }

        public int PremiumId { get; set; }


        // =====================================================
        // GRATUITO
        // =====================================================

        [Display(Name = "Consultas mensais")]
        [Range(0, 100000)]
        public int FreeMonthlyLimit { get; set; }

        public bool FreeHasAds { get; set; }

        public bool FreeHasHistory { get; set; }

        public bool FreeCanDownloadRecipes { get; set; }

        public bool FreeIsActive { get; set; }


        // =====================================================
        // PREMIUM
        // =====================================================

        [Display(Name = "Consultas mensais")]
        [Range(0, 100000)]
        public int PremiumMonthlyLimit { get; set; }

        [Display(Name = "Preço mensal")]
        [Range(0, 100000)]
        public decimal PremiumPrice { get; set; }

        public bool PremiumHasAds { get; set; }

        public bool PremiumHasHistory { get; set; }

        public bool PremiumCanDownloadRecipes { get; set; }

        public bool PremiumIsActive { get; set; }
    }
}