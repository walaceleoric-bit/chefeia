using chefeia.Models;

namespace chefeia.Services
{
    public interface IAiUsageLimitService
    {
        Task<AiUsageLimitResult> ObterLimiteAsync(
            string planCode,
            string? userId = null
        );
    }
}