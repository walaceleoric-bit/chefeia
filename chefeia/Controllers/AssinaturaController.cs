using chefeia.Models;
using chefeia.Services;
using chefeia.Services.Asaas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace chefeia.Controllers
{
    [Authorize]
    public class AssinaturaController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IPlanService _planService;
        private readonly IAsaasService _asaasService;
        private readonly ILogger<AssinaturaController> _logger;


        public AssinaturaController(
            UserManager<AppUser> userManager,
            IPlanService planService,
            IAsaasService asaasService,
            ILogger<AssinaturaController> logger)
        {
            _userManager =
                userManager;

            _planService =
                planService;

            _asaasService =
                asaasService;

            _logger =
                logger;
        }


        // =====================================================
        // ASSINAR PREMIUM
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Premium()
        {
            // =================================================
            // USUÁRIO LOGADO
            // =================================================

            var usuario =
                await _userManager
                    .GetUserAsync(User);


            if (usuario == null)
            {
                return RedirectToAction(
                    "Login",
                    "Conta",
                    new
                    {
                        returnUrl =
                            Url.Action(
                                nameof(Premium),
                                "Assinatura"
                            )
                    }
                );
            }


            // =================================================
            // CONTA DESATIVADA
            // =================================================

            if (!usuario.IsActive)
            {
                return RedirectToAction(
                    "AcessoNegado",
                    "Conta"
                );
            }


            // =================================================
            // JÁ É PREMIUM
            // =================================================

            var planoAtual =
                string.IsNullOrWhiteSpace(
                    usuario.PlanCode
                )
                    ? "FREE"
                    : usuario.PlanCode
                        .Trim()
                        .ToUpperInvariant();


            if (planoAtual == "PREMIUM")
            {
                return RedirectToAction(
                    nameof(JaPremium)
                );
            }


            // =================================================
            // OBTER PLANO PREMIUM
            // =================================================

            var premium =
                await _planService
                    .ObterPorCodigoAsync(
                        "PREMIUM"
                    );


            if (premium == null)
            {
                TempData["Erro"] =
                    "O plano Premium não foi encontrado.";

                return RedirectToAction(
                    "Index",
                    "Home"
                );
            }


            if (!premium.IsActive)
            {
                TempData["Erro"] =
                    "O plano Premium está temporariamente indisponível.";

                return RedirectToAction(
                    "Index",
                    "Home"
                );
            }


            if (premium.Price <= 0)
            {
                TempData["Erro"] =
                    "O preço do plano Premium não está configurado corretamente.";

                return RedirectToAction(
                    "Index",
                    "Home"
                );
            }


            // =================================================
            // URLs DE RETORNO
            // =================================================

            var successUrl =
                Url.Action(
                    nameof(Sucesso),
                    "Assinatura",
                    null,
                    Request.Scheme
                );


            var cancelUrl =
                Url.Action(
                    nameof(Cancelado),
                    "Assinatura",
                    null,
                    Request.Scheme
                );


            var expiredUrl =
                Url.Action(
                    nameof(Expirado),
                    "Assinatura",
                    null,
                    Request.Scheme
                );


            if (
                string.IsNullOrWhiteSpace(
                    successUrl
                ) ||
                string.IsNullOrWhiteSpace(
                    cancelUrl
                ) ||
                string.IsNullOrWhiteSpace(
                    expiredUrl
                ))
            {
                TempData["Erro"] =
                    "Não foi possível gerar as URLs de retorno do pagamento.";

                return RedirectToAction(
                    "Index",
                    "Home"
                );
            }


            try
            {
                // =================================================
                // CRIAR CHECKOUT ASAAS
                // =================================================

                var checkoutUrl =
                    await _asaasService
                        .CriarCheckoutPremiumAsync(
                            usuario,
                            premium.Price,
                            successUrl,
                            cancelUrl,
                            expiredUrl
                        );


                // =================================================
                // REDIRECIONAR PARA ASAAS
                // =================================================

                return Redirect(
                    checkoutUrl
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao iniciar assinatura Premium para usuário {UserId}.",
                    usuario.Id
                );


                TempData["Erro"] =
                    "Não foi possível iniciar o pagamento do Premium agora.";

                return RedirectToAction(
                    "Index",
                    "Home"
                );
            }
        }


        // =====================================================
        // RETORNO: CHECKOUT CONCLUÍDO
        //
        // IMPORTANTE:
        // isso NÃO ativa o Premium.
        //
        // O Premium será ativado somente pelo webhook
        // do Asaas quando o pagamento for confirmado.
        // =====================================================

        [HttpGet]
        public IActionResult Sucesso()
        {
            return View();
        }


        // =====================================================
        // RETORNO: CHECKOUT CANCELADO
        // =====================================================

        [HttpGet]
        public IActionResult Cancelado()
        {
            return View();
        }


        // =====================================================
        // RETORNO: CHECKOUT EXPIRADO
        // =====================================================

        [HttpGet]
        public IActionResult Expirado()
        {
            return View();
        }


        // =====================================================
        // USUÁRIO JÁ É PREMIUM
        // =====================================================

        [HttpGet]
        public IActionResult JaPremium()
        {
            return View();
        }
    }
}