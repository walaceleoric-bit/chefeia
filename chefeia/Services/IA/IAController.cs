using chefeia.Models;
using chefeia.Services.AI;
using Microsoft.AspNetCore.Mvc;

namespace chefeia.Controllers.Api
{
    [ApiController]
    [Route("api/ai")]
    public class IAController : ControllerBase
    {
        private readonly IChefeIAService _chefeIAService;

        public IAController(IChefeIAService chefeIAService)
        {
            _chefeIAService = chefeIAService;
        }

        [HttpPost("sugerir-receita")]
        public async Task<IActionResult> SugerirReceita(
            [FromBody] ConsultaReceitaIA consulta
        )
        {
            var receita =
                await _chefeIAService.SugerirReceitaAsync(consulta);

            return Ok(receita);
        }
    }
}