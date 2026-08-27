using chefeia.Data;
using chefeia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace chefeia.Controllers
{
    public class ContaController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly AppDbContext _dbContext;

        public ContaController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            AppDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
        }


        // =====================================================
        // LOGIN
        // =====================================================

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(
            string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction(
                        "Index",
                        "Admin"
                    );
                }

                return RedirectToAction(
                    "Index",
                    "Home"
                );
            }

            return View(
                new LoginViewModel
                {
                    ReturnUrl = returnUrl
                }
            );
        }


        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email =
                model.Email
                    .Trim()
                    .ToLowerInvariant();

            var usuario =
                await _userManager
                    .FindByEmailAsync(email);

            if (usuario == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "E-mail ou senha inválidos."
                );

                return View(model);
            }

            if (!usuario.IsActive)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Esta conta está desativada."
                );

                return View(model);
            }

            var resultado =
                await _signInManager
                    .PasswordSignInAsync(
                        usuario,
                        model.Password,
                        model.RememberMe,
                        lockoutOnFailure: true
                    );

            if (resultado.IsLockedOut)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Sua conta foi temporariamente bloqueada por excesso de tentativas."
                );

                return View(model);
            }

            if (!resultado.Succeeded)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "E-mail ou senha inválidos."
                );

                return View(model);
            }

            usuario.LastLoginAt =
                DateTime.UtcNow;

            await _userManager
                .UpdateAsync(usuario);

            if (
                await _userManager
                    .IsInRoleAsync(
                        usuario,
                        "Admin"
                    ))
            {
                return RedirectToAction(
                    "Index",
                    "Admin"
                );
            }

            if (
                !string.IsNullOrWhiteSpace(
                    model.ReturnUrl
                ) &&
                Url.IsLocalUrl(
                    model.ReturnUrl
                ))
            {
                return LocalRedirect(
                    model.ReturnUrl
                );
            }

            return RedirectToAction(
                "Index",
                "Home"
            );
        }


        // =====================================================
        // CADASTRO GRATUITO
        // =====================================================

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Cadastro()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(
                    "Index",
                    "Home"
                );
            }

            return View(
                new RegisterViewModel()
            );
        }


        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cadastro(
            RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email =
                model.Email
                    .Trim()
                    .ToLowerInvariant();

            var existente =
                await _userManager
                    .FindByEmailAsync(email);

            if (existente != null)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "Já existe uma conta com este e-mail."
                );

                return View(model);
            }

            var usuario =
                new AppUser
                {
                    UserName = email,
                    Email = email,
                    Name = model.Name.Trim(),
                    PlanCode = "FREE",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true
                };

            var resultado =
                await _userManager
                    .CreateAsync(
                        usuario,
                        model.Password
                    );

            if (!resultado.Succeeded)
            {
                foreach (
                    var erro
                    in resultado.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        erro.Description
                    );
                }

                return View(model);
            }

            await _signInManager
                .SignInAsync(
                    usuario,
                    isPersistent: false
                );

            return RedirectToAction(
                "Index",
                "Home"
            );
        }


        // =====================================================
        // HISTÓRICO PREMIUM
        // =====================================================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Historico()
        {
            var usuario =
                await _userManager
                    .GetUserAsync(User);

            if (usuario == null)
            {
                return RedirectToAction(
                    nameof(Login)
                );
            }

            if (!usuario.IsActive)
            {
                await _signInManager
                    .SignOutAsync();

                return RedirectToAction(
                    nameof(Login)
                );
            }

            var plano =
                ObterPlanoUsuario(
                    usuario
                );

            if (plano != "PREMIUM")
            {
                return View(
                    "HistoricoPremiumNecessario"
                );
            }

            var registros =
                await _dbContext
                    .RecipeHistories
                    .AsNoTracking()
                    .Where(
                        x =>
                            x.UserId ==
                            usuario.Id
                    )
                    .OrderByDescending(
                        x => x.CreatedAt
                    )
                    .ToListAsync();

            var model =
                new List<RecipeHistoryViewModel>();

            foreach (var item in registros)
            {
                var ingredientes =
                    DeserializarLista(
                        item.IngredientsJson
                    );

                var passos =
                    DeserializarLista(
                        item.StepsJson
                    );

                model.Add(
                    new RecipeHistoryViewModel
                    {
                        Id =
                            item.Id,

                        Name =
                            item.Name,

                        Country =
                            item.Country,

                        Category =
                            item.Category,

                        Description =
                            item.Description,

                        PreparationMinutes =
                            item.PreparationMinutes,

                        Servings =
                            item.Servings,

                        Ingredients =
                            ingredientes,

                        Steps =
                            passos,

                        RequestedIngredients =
                            item.RequestedIngredients,

                        Preference =
                            item.Preference,

                        CreatedAt =
                            item.CreatedAt
                    }
                );
            }

            return View(model);
        }


        // =====================================================
        // EXCLUIR RECEITA DO HISTÓRICO
        // =====================================================

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirReceita(
            int id)
        {
            var usuario =
                await _userManager
                    .GetUserAsync(User);

            if (usuario == null)
            {
                return RedirectToAction(
                    nameof(Login)
                );
            }

            if (!usuario.IsActive)
            {
                await _signInManager
                    .SignOutAsync();

                return RedirectToAction(
                    nameof(Login)
                );
            }

            var plano =
                ObterPlanoUsuario(
                    usuario
                );

            if (plano != "PREMIUM")
            {
                return View(
                    "HistoricoPremiumNecessario"
                );
            }

            var receita =
                await _dbContext
                    .RecipeHistories
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id == id &&
                            x.UserId ==
                            usuario.Id
                    );

            if (receita == null)
            {
                return NotFound();
            }

            _dbContext
                .RecipeHistories
                .Remove(receita);

            await _dbContext
                .SaveChangesAsync();

            TempData["Sucesso"] =
                "Receita excluída com sucesso.";

            return RedirectToAction(
                nameof(Historico)
            );
        }


        // =====================================================
        // DOWNLOAD DA RECEITA - SOMENTE PREMIUM
        // =====================================================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DownloadReceita(
            int id)
        {
            var usuario =
                await _userManager
                    .GetUserAsync(User);

            if (usuario == null)
            {
                return RedirectToAction(
                    nameof(Login)
                );
            }

            if (!usuario.IsActive)
            {
                await _signInManager
                    .SignOutAsync();

                return RedirectToAction(
                    nameof(Login)
                );
            }

            var plano =
                ObterPlanoUsuario(
                    usuario
                );

            if (plano != "PREMIUM")
            {
                return View(
                    "HistoricoPremiumNecessario"
                );
            }

            var receita =
                await _dbContext
                    .RecipeHistories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id == id &&
                            x.UserId ==
                            usuario.Id
                    );

            if (receita == null)
            {
                return NotFound();
            }

            var ingredientes =
                DeserializarLista(
                    receita.IngredientsJson
                );

            var passos =
                DeserializarLista(
                    receita.StepsJson
                );

            var texto =
                new StringBuilder();

            texto.AppendLine(
                "========================================"
            );

            texto.AppendLine(
                "              CHEFE IA"
            );

            texto.AppendLine(
                "========================================"
            );

            texto.AppendLine();

            texto.AppendLine(
                receita.Name
            );

            texto.AppendLine();

            if (
                !string.IsNullOrWhiteSpace(
                    receita.Description))
            {
                texto.AppendLine(
                    receita.Description
                );

                texto.AppendLine();
            }

            texto.AppendLine(
                "----------------------------------------"
            );

            texto.AppendLine(
                "INFORMAÇÕES"
            );

            texto.AppendLine(
                "----------------------------------------"
            );

            if (
                !string.IsNullOrWhiteSpace(
                    receita.Country))
            {
                texto.AppendLine(
                    $"Origem: {receita.Country}"
                );
            }

            if (
                !string.IsNullOrWhiteSpace(
                    receita.Category))
            {
                texto.AppendLine(
                    $"Categoria: {receita.Category}"
                );
            }

            texto.AppendLine(
                $"Tempo de preparo: {receita.PreparationMinutes} minutos"
            );

            texto.AppendLine(
                $"Porções: {receita.Servings}"
            );

            texto.AppendLine();

            texto.AppendLine(
                "----------------------------------------"
            );

            texto.AppendLine(
                "INGREDIENTES"
            );

            texto.AppendLine(
                "----------------------------------------"
            );

            if (ingredientes.Count == 0)
            {
                texto.AppendLine(
                    "Nenhum ingrediente informado."
                );
            }
            else
            {
                foreach (
                    var ingrediente
                    in ingredientes)
                {
                    texto.AppendLine(
                        $"• {ingrediente}"
                    );
                }
            }

            texto.AppendLine();

            texto.AppendLine(
                "----------------------------------------"
            );

            texto.AppendLine(
                "MODO DE PREPARO"
            );

            texto.AppendLine(
                "----------------------------------------"
            );

            if (passos.Count == 0)
            {
                texto.AppendLine(
                    "Nenhum passo informado."
                );
            }
            else
            {
                for (
                    var i = 0;
                    i < passos.Count;
                    i++)
                {
                    texto.AppendLine(
                        $"{i + 1}. {passos[i]}"
                    );

                    texto.AppendLine();
                }
            }

            if (
                !string.IsNullOrWhiteSpace(
                    receita.RequestedIngredients))
            {
                texto.AppendLine(
                    "----------------------------------------"
                );

                texto.AppendLine(
                    "CONSULTA ORIGINAL"
                );

                texto.AppendLine(
                    "----------------------------------------"
                );

                texto.AppendLine(
                    "Ingredientes informados:"
                );

                texto.AppendLine(
                    receita.RequestedIngredients
                );

                if (
                    !string.IsNullOrWhiteSpace(
                        receita.Preference))
                {
                    texto.AppendLine();

                    texto.AppendLine(
                        $"Preferência: {receita.Preference}"
                    );
                }

                texto.AppendLine();
            }

            texto.AppendLine(
                "========================================"
            );

            texto.AppendLine(
                "Receita criada com o Chefe IA"
            );

            texto.AppendLine(
                $"Data: {receita.CreatedAt.ToLocalTime():dd/MM/yyyy HH:mm}"
            );

            texto.AppendLine(
                "========================================"
            );

            var utf8 =
                new UTF8Encoding(
                    encoderShouldEmitUTF8Identifier:
                        true
                );

            var conteudo =
                utf8.GetPreamble()
                    .Concat(
                        utf8.GetBytes(
                            texto.ToString()
                        )
                    )
                    .ToArray();

            var nomeArquivo =
                CriarNomeArquivoSeguro(
                    receita.Name
                );

            return File(
                conteudo,
                "text/plain; charset=utf-8",
                $"{nomeArquivo}.txt"
            );
        }


        // =====================================================
        // LOGOUT
        // =====================================================

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager
                .SignOutAsync();

            return RedirectToAction(
                "Index",
                "Home"
            );
        }


        // =====================================================
        // ACESSO NEGADO
        // =====================================================

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AcessoNegado()
        {
            return View();
        }


        // =====================================================
        // OBTER PLANO DO USUÁRIO
        // =====================================================

        private static string ObterPlanoUsuario(
            AppUser usuario)
        {
            if (
                string.IsNullOrWhiteSpace(
                    usuario.PlanCode))
            {
                return "FREE";
            }

            var plano =
                usuario.PlanCode
                    .Trim()
                    .ToUpperInvariant();

            if (
                plano != "FREE" &&
                plano != "PREMIUM")
            {
                return "FREE";
            }

            return plano;
        }


        // =====================================================
        // DESERIALIZAR LISTA JSON
        // =====================================================

        private static List<string> DeserializarLista(
            string? json)
        {
            if (
                string.IsNullOrWhiteSpace(
                    json))
            {
                return new List<string>();
            }

            try
            {
                return JsonSerializer
                    .Deserialize<List<string>>(
                        json
                    )
                    ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }


        // =====================================================
        // NOME SEGURO PARA DOWNLOAD
        // =====================================================

        private static string CriarNomeArquivoSeguro(
            string? nome)
        {
            var nomeArquivo =
                string.IsNullOrWhiteSpace(nome)
                    ? "receita-chefe-ia"
                    : nome.Trim();

            foreach (
                var caractere
                in Path.GetInvalidFileNameChars())
            {
                nomeArquivo =
                    nomeArquivo.Replace(
                        caractere,
                        '-'
                    );
            }

            nomeArquivo =
                nomeArquivo.Replace(
                    " ",
                    "-"
                );

            while (
                nomeArquivo.Contains(
                    "--"
                ))
            {
                nomeArquivo =
                    nomeArquivo.Replace(
                        "--",
                        "-"
                    );
            }

            nomeArquivo =
                nomeArquivo.Trim(
                    '-',
                    '.',
                    ' '
                );

            if (
                string.IsNullOrWhiteSpace(
                    nomeArquivo))
            {
                nomeArquivo =
                    "receita-chefe-ia";
            }

            if (
                nomeArquivo.Length > 80)
            {
                nomeArquivo =
                    nomeArquivo.Substring(
                        0,
                        80
                    );
            }

            return nomeArquivo;
        }
    }
}