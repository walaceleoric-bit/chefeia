using chefeia.Models;
using Microsoft.AspNetCore.Mvc;

namespace chefeia.Controllers.Api
{
    [ApiController]
    [Route("api/paises")]
    public class PaisesController : ControllerBase
    {
        [HttpGet]
        public IActionResult ObterTodos()
        {
            var paises = new List<Pais>
            {
                new() { Id = 1, Nome = "Brasil", Codigo = "BR", Bandeira = "🇧🇷" },
                new() { Id = 2, Nome = "Itália", Codigo = "IT", Bandeira = "🇮🇹" },
                new() { Id = 3, Nome = "México", Codigo = "MX", Bandeira = "🇲🇽" },
                new() { Id = 4, Nome = "Japão", Codigo = "JP", Bandeira = "🇯🇵" },
                new() { Id = 5, Nome = "França", Codigo = "FR", Bandeira = "🇫🇷" },
                new() { Id = 6, Nome = "Portugal", Codigo = "PT", Bandeira = "🇵🇹" }
            };

            return Ok(paises);
        }
    }
}