using chefeia.Models;

namespace chefeia.Services
{
    public interface ISiteSettingsService
    {
        Task<SiteSettings> ObterAsync();

        Task SalvarAsync(
            SiteSettings settings
        );
    }
}