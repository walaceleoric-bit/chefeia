using chefeia.Services;
using Microsoft.AspNetCore.Mvc;

namespace chefeia.Controllers.Api
{
    [ApiController]
    [Route("api/receitas")]
    public class ReceitasController : ControllerBase
    {
        private readonly IReceitaService _receitaService;

        public ReceitasController(IReceitaService receitaService)
        {
            _receitaService = receitaService;
        }

        [HttpGet]
        public IActionResult ObterTodas()
        {
            var receitas = _receitaService.ObterDestaques();

            return Ok(receitas);
        }

        [HttpGet("buscar")]
        public IActionResult Buscar([FromQuery] string termo)
        {
            var receitas = _receitaService.Buscar(termo);

            return Ok(receitas);
        }
    }
}