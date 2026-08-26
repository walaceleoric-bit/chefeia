using chefeia.Models;

namespace chefeia.Services
{
    public interface IPlanService
    {
        Task<Plan?> ObterPorCodigoAsync(string codigo);

        Task<List<Plan>> ObterTodosAsync();

        Task AtualizarAsync(Plan plano);
    }
}