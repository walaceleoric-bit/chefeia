using chefeia.Models;

namespace chefeia.Services.AI
{
    public interface IChefeIAService
    {
        Task<ReceitaIA> SugerirReceitaAsync(
            ConsultaReceitaIA consulta
        );
    }
}