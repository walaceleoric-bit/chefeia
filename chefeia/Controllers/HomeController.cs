using chefeia.Services;
using Microsoft.AspNetCore.Mvc;

namespace chefeia.Controllers
{
    public class HomeController : Controller
    {
        private readonly IReceitaService _receitaService;

        public HomeController(IReceitaService receitaService)
        {
            _receitaService = receitaService;
        }

        public IActionResult Index()
        {
            var receitas = _receitaService.ObterDestaques();

            return View(receitas);
        }
    }
}