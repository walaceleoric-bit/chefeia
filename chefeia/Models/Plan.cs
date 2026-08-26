namespace chefeia.Models
{
    public class Plan
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int MonthlyAiLimit { get; set; }

        public decimal Price { get; set; }

        public bool HasAds { get; set; }

        public bool IsActive { get; set; } = true;
    }
}