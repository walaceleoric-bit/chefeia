namespace chefeia.Models
{
    public class AdminDashboardViewModel
    {
        public string SiteName { get; set; } =
            string.Empty;

        public int FreeMonthlyLimit { get; set; }

        public int PremiumMonthlyLimit { get; set; }

        public decimal PremiumPrice { get; set; }

        public string HeroTitle { get; set; } =
            string.Empty;

        public string HeroImageUrl { get; set; } =
            string.Empty;

        public string FeaturedRecipeTitle { get; set; } =
            string.Empty;

        public string FeaturedRecipeImageUrl { get; set; } =
            string.Empty;
    }
}