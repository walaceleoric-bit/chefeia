using chefeia.Data;
using chefeia.Models;
using Microsoft.EntityFrameworkCore;

namespace chefeia.Services
{
    public class PlanService : IPlanService
    {
        private readonly AppDbContext _dbContext;

        public PlanService(
            AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Plan?> ObterPorCodigoAsync(
            string codigo)
        {
            return await _dbContext.Plans
                .FirstOrDefaultAsync(
                    x => x.Code == codigo
                );
        }

        public async Task<List<Plan>>
            ObterTodosAsync()
        {
            return await _dbContext.Plans
                .OrderBy(x => x.Id)
                .ToListAsync();
        }

        public async Task AtualizarAsync(
            Plan plano)
        {
            var atual =
                await _dbContext.Plans
                    .FirstOrDefaultAsync(
                        x => x.Id == plano.Id
                    );

            if (atual == null)
            {
                throw new InvalidOperationException(
                    "Plano não encontrado."
                );
            }

            atual.Name =
                plano.Name;

            atual.MonthlyAiLimit =
                plano.MonthlyAiLimit;

            atual.Price =
                plano.Price;

            atual.HasAds =
                plano.HasAds;

            atual.HasHistory =
                plano.HasHistory;

            atual.CanDownloadRecipes =
                plano.CanDownloadRecipes;

            atual.IsActive =
                plano.IsActive;

            await _dbContext.SaveChangesAsync();
        }
    }
}