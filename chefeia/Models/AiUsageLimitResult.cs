namespace chefeia.Models
{
    public class AiUsageLimitResult
    {
        public string PlanCode { get; set; } = string.Empty;

        public string PlanName { get; set; } = string.Empty;

        public int MonthlyLimit { get; set; }

        public int UsedThisMonth { get; set; }

        public int Remaining { get; set; }

        public bool CanUse { get; set; }

        public DateTime PeriodStartUtc { get; set; }

        public DateTime PeriodEndUtc { get; set; }
    }
}