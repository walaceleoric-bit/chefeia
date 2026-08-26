using chefeia.Models;
using chefeia.Services;
using chefeia.Services.AI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace chefeia.Controllers.Api
{
    [ApiController]
    [Route("api/ai")]
    public class IAController : ControllerBase
    {
        private readonly IChefeIAService _chefeIAService;

        private readonly IAiUsageLimitService
            _aiUsageLimitService;

        private readonly UserManager<AppUser>
            _userManager;


        public IAController(
            IChefeIAService chefeIAService,
            IAiUsageLimitService aiUsageLimitService,
            UserManager<AppUser> userManager)
        {
            _chefeIAService =
                chefeIAService;

            _aiUsageLimitService =
                aiUsageLimitService;

            _userManager =
                userManager;
        }


        // =====================================================
        // SUGERIR RECEITA COM IA
        // =====================================================

        [Authorize]
        [HttpPost("sugerir-receita")]
        public async Task<IActionResult> SugerirReceita(
            [FromBody] ConsultaReceitaIA consulta)
        {
            // =================================================
            // USUÁRIO LOGADO
            // =================================================

            var usuario =
                await _userManager
                    .GetUserAsync(User);


            if (usuario == null)
            {
                return Unauthorized(
                    new
                    {
                        success = false,

                        requiresLogin = true,

                        message =
                            "Faça login para criar sua receita."
                    }
                );
            }


            // =================================================
            // CONTA DESATIVADA
            // =================================================

            if (!usuario.IsActive)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        success = false,

                        message =
                            "Sua conta está desativada."
                    }
                );
            }


            // =================================================
            // PLANO
            // =================================================

            var planCode =
                string.IsNullOrWhiteSpace(
                    usuario.PlanCode
                )
                    ? "FREE"
                    : usuario.PlanCode
                        .Trim()
                        .ToUpperInvariant();


            // =================================================
            // CONSULTAR LIMITE
            // =================================================

            var limite =
                await _aiUsageLimitService
                    .ObterLimiteAsync(
                        planCode,
                        usuario.Id
                    );


            // =================================================
            // LIMITE ATINGIDO
            // =================================================

            if (!limite.CanUse)
            {
                return StatusCode(
                    StatusCodes.Status429TooManyRequests,
                    new
                    {
                        success = false,

                        limitReached = true,

                        plan =
                            limite.PlanCode,

                        planName =
                            limite.PlanName,

                        used =
                            limite.UsedThisMonth,

                        limit =
                            limite.MonthlyLimit,

                        remaining =
                            limite.Remaining,

                        message =
                            planCode == "FREE"
                                ? "Você atingiu o limite mensal do plano Gratuito."
                                : "Você atingiu o limite mensal do seu plano Premium."
                    }
                );
            }


            // =================================================
            // CHAMAR IA
            // =================================================

            var receita =
                await _chefeIAService
                    .SugerirReceitaAsync(
                        consulta
                    );


            // =================================================
            // RETORNAR RECEITA + INFORMAÇÕES DO PLANO
            // =================================================

            return Ok(
                new
                {
                    success = true,

                    recipe =
                        receita,

                    usage =
                        new
                        {
                            plan =
                                limite.PlanCode,

                            planName =
                                limite.PlanName,

                            used =
                                limite.UsedThisMonth + 1,

                            limit =
                                limite.MonthlyLimit,

                            remaining =
                                Math.Max(
                                    limite.Remaining - 1,
                                    0
                                )
                        }
                }
            );
        }
    }
}