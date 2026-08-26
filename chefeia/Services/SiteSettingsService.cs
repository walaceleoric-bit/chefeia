using chefeia.Data;
using chefeia.Models;
using Microsoft.EntityFrameworkCore;

namespace chefeia.Services
{
    public class SiteSettingsService
        : ISiteSettingsService
    {
        private readonly AppDbContext _context;

        public SiteSettingsService(
            AppDbContext context)
        {
            _context = context;
        }

        public async Task<SiteSettings> ObterAsync()
        {
            var settings =
                await _context.SiteSettings
                    .FirstOrDefaultAsync(
                        x => x.Id == 1
                    );

            if (settings != null)
            {
                return settings;
            }

            settings =
                new SiteSettings
                {
                    Id = 1
                };

            _context.SiteSettings.Add(settings);

            await _context.SaveChangesAsync();

            return settings;
        }

        public async Task SalvarAsync(
            SiteSettings settings)
        {
            var atual =
                await _context.SiteSettings
                    .FirstOrDefaultAsync(
                        x => x.Id == 1
                    );

            if (atual == null)
            {
                settings.Id = 1;

                _context.SiteSettings.Add(
                    settings
                );
            }
            else
            {
                atual.SiteName =
                    settings.SiteName;

                atual.SiteSlogan =
                    settings.SiteSlogan;

                atual.HeroTitle =
                    settings.HeroTitle;

                atual.HeroSubtitle =
                    settings.HeroSubtitle;

                atual.HeroImageUrl =
                    settings.HeroImageUrl;

                atual.SearchPlaceholder =
                    settings.SearchPlaceholder;

                atual.Feature1Emoji =
                    settings.Feature1Emoji;

                atual.Feature1Title =
                    settings.Feature1Title;

                atual.Feature1Text =
                    settings.Feature1Text;

                atual.Feature2Emoji =
                    settings.Feature2Emoji;

                atual.Feature2Title =
                    settings.Feature2Title;

                atual.Feature2Text =
                    settings.Feature2Text;

                atual.Feature3Emoji =
                    settings.Feature3Emoji;

                atual.Feature3Title =
                    settings.Feature3Title;

                atual.Feature3Text =
                    settings.Feature3Text;

                atual.Feature4Emoji =
                    settings.Feature4Emoji;

                atual.Feature4Title =
                    settings.Feature4Title;

                atual.Feature4Text =
                    settings.Feature4Text;

                atual.FreeMonthlyLimit =
                    settings.FreeMonthlyLimit;

                atual.PremiumMonthlyLimit =
                    settings.PremiumMonthlyLimit;

                atual.PremiumPrice =
                    settings.PremiumPrice;

                atual.FeaturedRecipeTitle =
                    settings.FeaturedRecipeTitle;

                atual.FeaturedRecipeImageUrl =
                    settings.FeaturedRecipeImageUrl;

                atual.FeaturedRecipeCountry =
                    settings.FeaturedRecipeCountry;

                atual.FeaturedRecipeMinutes =
                    settings.FeaturedRecipeMinutes;

                atual.FeaturedRecipeServings =
                    settings.FeaturedRecipeServings;
            }

            await _context.SaveChangesAsync();
        }
    }
}