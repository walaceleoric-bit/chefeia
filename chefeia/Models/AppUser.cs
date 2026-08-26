using Microsoft.AspNetCore.Identity;

namespace chefeia.Models
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;

        public string PlanCode { get; set; } = "FREE";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } =
            DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }
    }
}