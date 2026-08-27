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
            // VALIDAR INGREDIENTES
            // =================================================

            if (
                consulta.Ingredientes == null ||
                !consulta.Ingredientes.Any(
                    x =>
                        !string.IsNullOrWhiteSpace(x))
            )
            {
                return BadRequest(
                    new
                    {
                        success = false,

                        message =
                            "Informe pelo menos um ingrediente."
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


            if (
                planCode != "FREE" &&
                planCode != "PREMIUM"
            )
            {
                planCode =
                    "FREE";
            }


            // =================================================
            // CONSULTAR LIMITE INTERNO DO USUÁRIO
            // =================================================

            var limite =
                await _aiUsageLimitService
                    .ObterLimiteAsync(
                        planCode,
                        usuario.Id
                    );


            // =================================================
            // LIMITE INTERNO ATINGIDO
            // =================================================

            if (!limite.CanUse)
            {
                return StatusCode(
                    StatusCodes.Status429TooManyRequests,
                    new
                    {
                        success = false,

                        limitReached = true,

                        externalLimit = false,

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


            try
            {
                // =================================================
                // CHAMAR IA
                // =================================================

                var resultado =
                    await _chefeIAService
                        .SugerirReceitaAsync(
                            consulta
                        );


                // =================================================
                // CONSULTAR LIMITE NOVAMENTE
                // APÓS A CONSULTA TER SIDO SALVA
                // =================================================

                var limiteAtualizado =
                    await _aiUsageLimitService
                        .ObterLimiteAsync(
                            planCode,
                            usuario.Id
                        );


                // =================================================
                // IA GEROU RECEITA
                // =================================================

                if (resultado.TemReceita)
                {
                    return Ok(
                        new
                        {
                            success = true,

                            hasRecipe = true,

                            responseType =
                                "RECEITA",

                            recipe =
                                resultado,

                            usage =
                                new
                                {
                                    plan =
                                        limiteAtualizado.PlanCode,

                                    planName =
                                        limiteAtualizado.PlanName,

                                    used =
                                        limiteAtualizado.UsedThisMonth,

                                    limit =
                                        limiteAtualizado.MonthlyLimit,

                                    remaining =
                                        limiteAtualizado.Remaining
                                }
                        }
                    );
                }


                // =================================================
                // IA NÃO RECOMENDA RECEITA
                // =================================================

                return Ok(
                    new
                    {
                        success = true,

                        hasRecipe = false,

                        responseType =
                            resultado.TipoResposta,

                        message =
                            resultado.Mensagem,

                        suggestions =
                            resultado.Sugestoes,

                        recipe =
                            (object?)null,

                        usage =
                            new
                            {
                                plan =
                                    limiteAtualizado.PlanCode,

                                planName =
                                    limiteAtualizado.PlanName,

                                used =
                                    limiteAtualizado.UsedThisMonth,

                                limit =
                                    limiteAtualizado.MonthlyLimit,

                                remaining =
                                    limiteAtualizado.Remaining
                            }
                    }
                );
            }
            catch (InvalidOperationException ex)
            {
                // =================================================
                // LIMITE / FALHA DA RAPIDAPI
                // =================================================

                if (
                    ex.Message.Contains(
                        "limite de uso",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return StatusCode(
                        StatusCodes.Status503ServiceUnavailable,
                        new
                        {
                            success = false,

                            externalLimit = true,

                            limitReached = false,

                            message =
                                "O serviço de inteligência artificial está temporariamente indisponível por limite da API externa. Tente novamente mais tarde."
                        }
                    );
                }


                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        success = false,

                        externalLimit = false,

                        message =
                            ex.Message
                    }
                );
            }
            catch (HttpRequestException)
            {
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    new
                    {
                        success = false,

                        externalLimit = true,

                        limitReached = false,

                        message =
                            "Não foi possível conectar ao serviço de inteligência artificial agora. Tente novamente em alguns minutos."
                    }
                );
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        success = false,

                        externalLimit = false,

                        message =
                            "Ocorreu um erro ao processar sua consulta."
                    }
                );
            }
        }
    }
}