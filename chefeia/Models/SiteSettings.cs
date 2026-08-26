using System.ComponentModel.DataAnnotations;

namespace chefeia.Models
{
    public class SiteSettings
    {
        public int Id { get; set; }

        [Required]
        public string SiteName { get; set; } = "Chefe IA";

        public string SiteSlogan { get; set; } =
            "Receitas que surpreendem!";

        public string HeroTitle { get; set; } =
            "Sua próxima receita começa aqui";

        public string HeroSubtitle { get; set; } =
            "Digite os ingredientes que você tem em casa e deixe o Chefe IA criar algo incrível para você!";

        public string HeroImageUrl { get; set; } =
            "https://images.unsplash.com/photo-1604908176997-125f25cc6f3d";

        public string SearchPlaceholder { get; set; } =
            "Ex: frango, batata, tomate...";

        public string Feature1Emoji { get; set; } = "🥗";
        public string Feature1Title { get; set; } = "Receitas personalizadas";
        public string Feature1Text { get; set; } =
            "Criadas sob medida com os ingredientes que você tem.";

        public string Feature2Emoji { get; set; } = "⏱️";
        public string Feature2Title { get; set; } = "Rápido e prático";
        public string Feature2Text { get; set; } =
            "Receitas prontas em segundos para facilitar seu dia.";

        public string Feature3Emoji { get; set; } = "🌎";
        public string Feature3Title { get; set; } = "Cozinha do mundo";
        public string Feature3Text { get; set; } =
            "Explore sabores de diferentes países e culturas.";

        public string Feature4Emoji { get; set; } = "❤️";
        public string Feature4Title { get; set; } = "Feito com carinho";
        public string Feature4Text { get; set; } =
            "Descubra novas combinações e experiências.";

        public int FreeMonthlyLimit { get; set; } = 3;

        public int PremiumMonthlyLimit { get; set; } = 50;

        public decimal PremiumPrice { get; set; } = 39.90m;

        public string FeaturedRecipeTitle { get; set; } =
            "Frango Cremoso com Batatas";

        public string FeaturedRecipeImageUrl { get; set; } =
            "https://images.unsplash.com/photo-1604908176997-125f25cc6f3d";

        public string FeaturedRecipeCountry { get; set; } =
            "Brasileira";

        public int FeaturedRecipeMinutes { get; set; } = 35;

        public int FeaturedRecipeServings { get; set; } = 4;
    }
}