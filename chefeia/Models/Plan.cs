using System.ComponentModel.DataAnnotations;

namespace chefeia.Models
{
    public class Plan
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } =
            string.Empty;

        [Required]
        [MaxLength(30)]
        public string Code { get; set; } =
            string.Empty;

        public int MonthlyAiLimit { get; set; }

        public decimal Price { get; set; }

        public bool HasAds { get; set; }

        public bool HasHistory { get; set; }

        public bool CanDownloadRecipes { get; set; }

        public bool IsActive { get; set; } = true;
    }
}