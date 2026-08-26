using chefeia.Data;
using chefeia.Models;
using Microsoft.EntityFrameworkCore;

namespace chefeia.Services
{
    public class AiUsageLimitService : IAiUsageLimitService
    {
        private readonly AppDbContext _dbContext;
        private readonly IPlanService _planService;

        public AiUsageLimitService(
            AppDbContext dbContext,
            IPlanService planService)
        {
            _dbContext = dbContext;
            _planService = planService;
        }

        public async Task<AiUsageLimitResult> ObterLimiteAsync(
            string planCode,
            string? userId = null)
        {
            var plano =
                await _planService
                    .ObterPorCodigoAsync(planCode);

            if (plano == null)
            {
                throw new InvalidOperationException(
                    $"Plano '{planCode}' não encontrado."
                );
            }

            var agoraUtc =
                DateTime.UtcNow;

            var inicioMesUtc =
                new DateTime(
                    agoraUtc.Year,
                    agoraUtc.Month,
                    1,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc
                );

            var inicioProximoMesUtc =
                inicioMesUtc.AddMonths(1);

            var query =
                _dbContext.AiUsages
                    .AsNoTracking()
                    .Where(
                        x =>
                            x.CreatedAt >= inicioMesUtc &&
                            x.CreatedAt < inicioProximoMesUtc &&
                            x.Success
                    );

            if (!string.IsNullOrWhiteSpace(userId))
            {
                query =
                    query.Where(
                        x => x.UserId == userId
                    );
            }
            else
            {
                query =
                    query.Where(
                        x => x.PlanName == planCode
                    );
            }

            var usadas =
                await query.CountAsync();

            var limite =
                plano.MonthlyAiLimit;

            var restantes =
                limite - usadas;

            if (restantes < 0)
            {
                restantes = 0;
            }

            var podeUsar =
                plano.IsActive &&
                usadas < limite;

            return new AiUsageLimitResult
            {
                PlanCode =
                    plano.Code,

                PlanName =
                    plano.Name,

                MonthlyLimit =
                    limite,

                UsedThisMonth =
                    usadas,

                Remaining =
                    restantes,

                CanUse =
                    podeUsar,

                PeriodStartUtc =
                    inicioMesUtc,

                PeriodEndUtc =
                    inicioProximoMesUtc
            };
        }
    }
}