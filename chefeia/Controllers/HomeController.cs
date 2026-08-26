using chefeia.Models;
using chefeia.Services;
using Microsoft.AspNetCore.Mvc;

namespace chefeia.Controllers
{
    public class HomeController : Controller
    {
        private readonly IReceitaService _receitaService;

        private readonly ISiteSettingsService
            _siteSettingsService;

        public HomeController(
            IReceitaService receitaService,
            ISiteSettingsService siteSettingsService)
        {
            _receitaService =
                receitaService;

            _siteSettingsService =
                siteSettingsService;
        }

        public async Task<IActionResult> Index()
        {
            var settings =
                await _siteSettingsService
                    .ObterAsync();

            var receitas =
                _receitaService
                    .ObterDestaques();

            var model =
                new HomeViewModel
                {
                    Settings = settings,
                    Receitas = receitas
                };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}