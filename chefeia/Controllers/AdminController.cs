using chefeia.Data;
using chefeia.Models;
using chefeia.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace chefeia.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ISiteSettingsService _siteSettingsService;
        private readonly IPlanService _planService;
        private readonly AppDbContext _dbContext;
        private readonly UserManager<AppUser> _userManager;

        public AdminController(
            ISiteSettingsService siteSettingsService,
            IPlanService planService,
            AppDbContext dbContext,
            UserManager<AppUser> userManager)
        {
            _siteSettingsService = siteSettingsService;
            _planService = planService;
            _dbContext = dbContext;
            _userManager = userManager;
        }


        // =====================================================
        // DASHBOARD
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var settings =
                await _siteSettingsService.ObterAsync();

            var gratuito =
                await _planService.ObterPorCodigoAsync("FREE");

            var premium =
                await _planService.ObterPorCodigoAsync("PREMIUM");

            var model =
                new AdminDashboardViewModel
                {
                    SiteName =
                        settings.SiteName,

                    FreeMonthlyLimit =
                        gratuito?.MonthlyAiLimit ?? 0,

                    PremiumMonthlyLimit =
                        premium?.MonthlyAiLimit ?? 0,

                    PremiumPrice =
                        premium?.Price ?? 0,

                    HeroTitle =
                        settings.HeroTitle
                };

            return View(model);
        }


        // =====================================================
        // CONFIGURAÇÕES DA HOME
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Configuracoes()
        {
            var settings =
                await _siteSettingsService.ObterAsync();

            return View(settings);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Configuracoes(
            SiteSettings model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _siteSettingsService
                .SalvarAsync(model);

            TempData["Sucesso"] =
                "Configurações salvas com sucesso.";

            return RedirectToAction(
                nameof(Configuracoes));
        }


        // =====================================================
        // PLANOS
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Planos()
        {
            var gratuito =
                await _planService.ObterPorCodigoAsync(
                    "FREE");

            var premium =
                await _planService.ObterPorCodigoAsync(
                    "PREMIUM");

            if (gratuito == null ||
                premium == null)
            {
                return Content(
                    "Os planos FREE e PREMIUM não foram encontrados.");
            }

            var model =
                new AdminPlanosViewModel
                {
                    FreeId =
                        gratuito.Id,

                    PremiumId =
                        premium.Id,

                    FreeMonthlyLimit =
                        gratuito.MonthlyAiLimit,

                    FreeHasAds =
                        gratuito.HasAds,

                    FreeHasHistory =
                        gratuito.HasHistory,

                    FreeCanDownloadRecipes =
                        gratuito.CanDownloadRecipes,

                    FreeIsActive =
                        gratuito.IsActive,

                    PremiumMonthlyLimit =
                        premium.MonthlyAiLimit,

                    PremiumPrice =
                        premium.Price,

                    PremiumHasAds =
                        premium.HasAds,

                    PremiumHasHistory =
                        premium.HasHistory,

                    PremiumCanDownloadRecipes =
                        premium.CanDownloadRecipes,

                    PremiumIsActive =
                        premium.IsActive
                };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Planos(
            AdminPlanosViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var gratuito =
                await _planService.ObterPorCodigoAsync(
                    "FREE");

            var premium =
                await _planService.ObterPorCodigoAsync(
                    "PREMIUM");

            if (gratuito == null ||
                premium == null)
            {
                return Content(
                    "Os planos FREE e PREMIUM não foram encontrados.");
            }


            // GRATUITO

            gratuito.MonthlyAiLimit =
                model.FreeMonthlyLimit;

            gratuito.Price = 0;

            gratuito.HasAds =
                model.FreeHasAds;

            gratuito.HasHistory =
                model.FreeHasHistory;

            gratuito.CanDownloadRecipes =
                model.FreeCanDownloadRecipes;

            gratuito.IsActive =
                model.FreeIsActive;

            await _planService
                .AtualizarAsync(gratuito);


            // PREMIUM

            premium.MonthlyAiLimit =
                model.PremiumMonthlyLimit;

            premium.Price =
                model.PremiumPrice;

            premium.HasAds =
                model.PremiumHasAds;

            premium.HasHistory =
                model.PremiumHasHistory;

            premium.CanDownloadRecipes =
                model.PremiumCanDownloadRecipes;

            premium.IsActive =
                model.PremiumIsActive;

            await _planService
                .AtualizarAsync(premium);


            // HOME

            var settings =
                await _siteSettingsService.ObterAsync();

            settings.FreeMonthlyLimit =
                gratuito.MonthlyAiLimit;

            settings.PremiumMonthlyLimit =
                premium.MonthlyAiLimit;

            settings.PremiumPrice =
                premium.Price;

            await _siteSettingsService
                .SalvarAsync(settings);


            TempData["Sucesso"] =
                "Planos atualizados com sucesso.";

            return RedirectToAction(
                nameof(Planos));
        }


        // =====================================================
        // LISTA DE USUÁRIOS
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Usuarios()
        {
            var usuarios =
                await _userManager.Users
                    .OrderByDescending(
                        x => x.CreatedAt)
                    .ToListAsync();

            var gratuito =
                await _planService.ObterPorCodigoAsync(
                    "FREE");

            var premium =
                await _planService.ObterPorCodigoAsync(
                    "PREMIUM");

            var agoraUtc =
                DateTime.UtcNow;

            var inicioMes =
                new DateTime(
                    agoraUtc.Year,
                    agoraUtc.Month,
                    1,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc);

            var lista =
                new List<AdminUsuarioViewModel>();


            foreach (var usuario in usuarios)
            {
                var isAdmin =
                    await _userManager
                        .IsInRoleAsync(
                            usuario,
                            "Admin");

                var usadas =
                    await _dbContext.AiUsages
                        .AsNoTracking()
                        .CountAsync(
                            x =>
                                x.UserId == usuario.Id &&
                                x.Success &&
                                x.CreatedAt >= inicioMes);

                var limite = 0;

                if (usuario.PlanCode == "PREMIUM")
                {
                    limite =
                        premium?.MonthlyAiLimit ?? 0;
                }
                else
                {
                    limite =
                        gratuito?.MonthlyAiLimit ?? 0;
                }

                var restantes =
                    Math.Max(
                        limite - usadas,
                        0);

                lista.Add(
                    new AdminUsuarioViewModel
                    {
                        Id =
                            usuario.Id,

                        Name =
                            usuario.Name,

                        Email =
                            usuario.Email ?? "",

                        PlanCode =
                            usuario.PlanCode,

                        IsActive =
                            usuario.IsActive,

                        CreatedAt =
                            usuario.CreatedAt,

                        LastLoginAt =
                            usuario.LastLoginAt,

                        IsAdmin =
                            isAdmin,

                        ConsultasUsadasMes =
                            usadas,

                        LimiteMensal =
                            limite,

                        ConsultasRestantes =
                            restantes
                    });
            }

            return View(lista);
        }


        // =====================================================
        // EDITAR USUÁRIO
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> EditarUsuario(
            string id)
        {
            var usuario =
                await _userManager.FindByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            var isAdmin =
                await _userManager
                    .IsInRoleAsync(
                        usuario,
                        "Admin");

            var model =
                new AdminEditarUsuarioViewModel
                {
                    Id =
                        usuario.Id,

                    Name =
                        usuario.Name,

                    Email =
                        usuario.Email ?? "",

                    PlanCode =
                        usuario.PlanCode,

                    IsActive =
                        usuario.IsActive,

                    IsAdmin =
                        isAdmin
                };

            return View(model);
        }


        // =====================================================
        // SALVAR USUÁRIO
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarUsuario(
            AdminEditarUsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario =
                await _userManager
                    .FindByIdAsync(model.Id);

            if (usuario == null)
            {
                return NotFound();
            }


            var plano =
                model.PlanCode
                    .Trim()
                    .ToUpperInvariant();

            if (plano != "FREE" &&
                plano != "PREMIUM")
            {
                ModelState.AddModelError(
                    nameof(model.PlanCode),
                    "Plano inválido.");

                return View(model);
            }


            var email =
                model.Email
                    .Trim()
                    .ToLowerInvariant();


            var usuarioComEmail =
                await _userManager
                    .FindByEmailAsync(email);

            if (
                usuarioComEmail != null &&
                usuarioComEmail.Id != usuario.Id)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Este e-mail já está sendo utilizado.");

                return View(model);
            }


            // =================================================
            // PROTEGER O ADMIN ATUAL
            // =================================================

            var usuarioAtualId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            var editandoProprioUsuario =
                usuarioAtualId == usuario.Id;


            if (editandoProprioUsuario &&
                !model.IsActive)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Você não pode desativar sua própria conta de administrador.");

                return View(model);
            }


            // =================================================
            // DADOS
            // =================================================

            usuario.Name =
                model.Name.Trim();

            usuario.PlanCode =
                plano;

            usuario.IsActive =
                model.IsActive;


            var resultadoEmail =
                await _userManager
                    .SetEmailAsync(
                        usuario,
                        email);

            if (!resultadoEmail.Succeeded)
            {
                foreach (
                    var erro
                    in resultadoEmail.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        erro.Description);
                }

                return View(model);
            }


            var resultadoUserName =
                await _userManager
                    .SetUserNameAsync(
                        usuario,
                        email);

            if (!resultadoUserName.Succeeded)
            {
                foreach (
                    var erro
                    in resultadoUserName.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        erro.Description);
                }

                return View(model);
            }


            var resultadoUpdate =
                await _userManager
                    .UpdateAsync(usuario);

            if (!resultadoUpdate.Succeeded)
            {
                foreach (
                    var erro
                    in resultadoUpdate.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        erro.Description);
                }

                return View(model);
            }


            // =================================================
            // ROLE ADMIN
            // =================================================

            var atualmenteAdmin =
                await _userManager
                    .IsInRoleAsync(
                        usuario,
                        "Admin");


            if (model.IsAdmin &&
                !atualmenteAdmin)
            {
                await _userManager
                    .AddToRoleAsync(
                        usuario,
                        "Admin");
            }


            if (!model.IsAdmin &&
                atualmenteAdmin)
            {
                if (editandoProprioUsuario)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Você não pode remover sua própria permissão de administrador.");

                    model.IsAdmin = true;

                    return View(model);
                }

                await _userManager
                    .RemoveFromRoleAsync(
                        usuario,
                        "Admin");
            }


            TempData["Sucesso"] =
                "Usuário atualizado com sucesso.";

            return RedirectToAction(
                nameof(Usuarios));
        }


        // =====================================================
        // EXCLUIR USUÁRIO
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirUsuario(
            string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["Erro"] =
                    "Usuário inválido.";

                return RedirectToAction(
                    nameof(Usuarios));
            }


            var usuarioAtualId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);


            // Nunca permitir que o administrador logado
            // exclua a própria conta.

            if (usuarioAtualId == id)
            {
                TempData["Erro"] =
                    "Você não pode excluir sua própria conta de administrador.";

                return RedirectToAction(
                    nameof(Usuarios));
            }


            var usuario =
                await _userManager
                    .FindByIdAsync(id);

            if (usuario == null)
            {
                TempData["Erro"] =
                    "O usuário não foi encontrado.";

                return RedirectToAction(
                    nameof(Usuarios));
            }


            // =================================================
            // TRANSAÇÃO
            // =================================================

            await using var transaction =
                await _dbContext.Database
                    .BeginTransactionAsync();

            try
            {
                // ---------------------------------------------
                // CONSUMO DA IA
                //
                // AiUsage possui UserId, mas não está
                // configurado com Cascade no AppDbContext.
                // Portanto apagamos manualmente.
                // ---------------------------------------------

                var consumos =
                    await _dbContext.AiUsages
                        .Where(
                            x => x.UserId == usuario.Id)
                        .ToListAsync();

                if (consumos.Count > 0)
                {
                    _dbContext.AiUsages
                        .RemoveRange(consumos);

                    await _dbContext
                        .SaveChangesAsync();
                }


                // ---------------------------------------------
                // HISTÓRICO E ASSINATURAS
                //
                // RecipeHistories e UserSubscriptions possuem
                // DeleteBehavior.Cascade.
                //
                // Ao excluir o AppUser, esses registros serão
                // removidos automaticamente pelo banco.
                // ---------------------------------------------


                // ---------------------------------------------
                // WEBHOOK ASAAS
                //
                // NÃO apagamos AsaasWebhookEvents.
                //
                // Eles são registros técnicos/históricos de
                // pagamento e não possuem FK com AppUser.
                // ---------------------------------------------


                // ---------------------------------------------
                // ASP.NET IDENTITY
                //
                // DeleteAsync também cuida dos registros
                // relacionados ao Identity, como roles,
                // claims, logins e tokens.
                // ---------------------------------------------

                var resultado =
                    await _userManager
                        .DeleteAsync(usuario);

                if (!resultado.Succeeded)
                {
                    await transaction
                        .RollbackAsync();

                    TempData["Erro"] =
                        "Não foi possível excluir o usuário: " +
                        string.Join(
                            " ",
                            resultado.Errors.Select(
                                x => x.Description));

                    return RedirectToAction(
                        nameof(Usuarios));
                }


                await transaction
                    .CommitAsync();


                TempData["Sucesso"] =
                    $"Usuário {usuario.Name} excluído com sucesso.";

                return RedirectToAction(
                    nameof(Usuarios));
            }
            catch (Exception)
            {
                await transaction
                    .RollbackAsync();

                TempData["Erro"] =
                    "Não foi possível excluir o usuário. Verifique os dados relacionados à conta.";

                return RedirectToAction(
                    nameof(Usuarios));
            }
        }


        // =====================================================
        // REDEFINIR SENHA
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> ResetSenha(
            string id)
        {
            var usuario =
                await _userManager
                    .FindByIdAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            var model =
                new AdminResetSenhaViewModel
                {
                    UserId =
                        usuario.Id,

                    UserName =
                        usuario.Name,

                    Email =
                        usuario.Email ?? ""
                };

            return View(model);
        }


        // =====================================================
        // SALVAR NOVA SENHA
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetSenha(
            AdminResetSenhaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var usuario =
                await _userManager
                    .FindByIdAsync(
                        model.UserId);

            if (usuario == null)
            {
                return NotFound();
            }


            var token =
                await _userManager
                    .GeneratePasswordResetTokenAsync(
                        usuario);


            var resultado =
                await _userManager
                    .ResetPasswordAsync(
                        usuario,
                        token,
                        model.NewPassword);


            if (!resultado.Succeeded)
            {
                foreach (
                    var erro
                    in resultado.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        erro.Description);
                }

                model.UserName =
                    usuario.Name;

                model.Email =
                    usuario.Email ?? "";

                return View(model);
            }


            await _userManager
                .UpdateSecurityStampAsync(
                    usuario);


            TempData["Sucesso"] =
                "Senha redefinida com sucesso.";

            return RedirectToAction(
                nameof(Usuarios));
        }


        // =====================================================
        // CONSUMO
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Consumo()
        {
            var agoraUtc =
                DateTime.UtcNow;

            var inicioHojeUtc =
                new DateTime(
                    agoraUtc.Year,
                    agoraUtc.Month,
                    agoraUtc.Day,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc);

            var inicioMesUtc =
                new DateTime(
                    agoraUtc.Year,
                    agoraUtc.Month,
                    1,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc);


            var consultasHoje =
                await _dbContext.AiUsages
                    .AsNoTracking()
                    .CountAsync(
                        x =>
                            x.CreatedAt >=
                            inicioHojeUtc);


            var consultasMes =
                await _dbContext.AiUsages
                    .AsNoTracking()
                    .CountAsync(
                        x =>
                            x.CreatedAt >=
                            inicioMesUtc);


            var consultasTotal =
                await _dbContext.AiUsages
                    .AsNoTracking()
                    .CountAsync();


            var sucessosMes =
                await _dbContext.AiUsages
                    .AsNoTracking()
                    .CountAsync(
                        x =>
                            x.CreatedAt >=
                            inicioMesUtc &&
                            x.Success);


            var errosMes =
                await _dbContext.AiUsages
                    .AsNoTracking()
                    .CountAsync(
                        x =>
                            x.CreatedAt >=
                            inicioMesUtc &&
                            !x.Success);


            var temposMes =
                await _dbContext.AiUsages
                    .AsNoTracking()
                    .Where(
                        x =>
                            x.CreatedAt >=
                            inicioMesUtc &&
                            x.Success)
                    .Select(
                        x => (long?)x.DurationMs)
                    .ToListAsync();


            double tempoMedioMs = 0;


            if (temposMes.Count > 0)
            {
                tempoMedioMs =
                    temposMes
                        .Where(
                            x => x.HasValue)
                        .Select(
                            x =>
                                (double)x!.Value)
                        .DefaultIfEmpty(0)
                        .Average();
            }


            var ultimoComLimites =
                await _dbContext.AiUsages
                    .AsNoTracking()
                    .Where(
                        x =>
                            x.RequestsLimit != null ||
                            x.RequestsRemaining != null ||
                            x.CreditLimit != null ||
                            x.CreditRemaining != null)
                    .OrderByDescending(
                        x => x.CreatedAt)
                    .FirstOrDefaultAsync();


            var ultimasConsultas =
                await _dbContext.AiUsages
                    .AsNoTracking()
                    .OrderByDescending(
                        x => x.CreatedAt)
                    .Take(50)
                    .ToListAsync();


            var model =
                new AdminConsumoViewModel
                {
                    ConsultasHoje =
                        consultasHoje,

                    ConsultasMes =
                        consultasMes,

                    ConsultasTotal =
                        consultasTotal,

                    SucessosMes =
                        sucessosMes,

                    ErrosMes =
                        errosMes,

                    TempoMedioMs =
                        tempoMedioMs,

                    RequestsLimit =
                        ultimoComLimites
                            ?.RequestsLimit,

                    RequestsRemaining =
                        ultimoComLimites
                            ?.RequestsRemaining,

                    CreditLimit =
                        ultimoComLimites
                            ?.CreditLimit,

                    CreditRemaining =
                        ultimoComLimites
                            ?.CreditRemaining,

                    UltimasConsultas =
                        ultimasConsultas
                };


            return View(model);
        }
    }
}