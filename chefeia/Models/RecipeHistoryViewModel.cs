namespace chefeia.Models
{
    public class RecipeHistoryViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } =
            string.Empty;

        public string Country { get; set; } =
            string.Empty;

        public string Category { get; set; } =
            string.Empty;

        public string Description { get; set; } =
            string.Empty;

        public int PreparationMinutes { get; set; }

        public int Servings { get; set; }

        public List<string> Ingredients { get; set; } =
            new();

        public List<string> Steps { get; set; } =
            new();

        public string RequestedIngredients { get; set; } =
            string.Empty;

        public string Preference { get; set; } =
            string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}