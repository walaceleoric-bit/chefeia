using Microsoft.AspNetCore.Mvc;

namespace chefeia.Controllers.Api
{
    [ApiController]
    [Route("api/ingredientes")]
    public class IngredientesController : ControllerBase
    {
        [HttpGet("buscar")]
        public IActionResult Buscar([FromQuery] string termo)
        {
            var ingredientes = new[]
            {
                "Leite condensado",
                "Chocolate",
                "Coco",
                "Ovos",
                "Farinha",
                "Leite",
                "Banana",
                "Morango"
            };

            var resultado = ingredientes
                .Where(i => i.Contains(
                    termo ?? "",
                    StringComparison.OrdinalIgnoreCase))
                .Take(10);

            return Ok(resultado);
        }
    }
}