using chefeia.Data;
using chefeia.Models;
using chefeia.Services;
using chefeia.Services.AI;
using chefeia.Services.Asaas;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder =
    WebApplication.CreateBuilder(args);


// =====================================================
// MVC
// =====================================================

builder.Services.AddControllersWithViews();


// =====================================================
// ACESSO AO USUÁRIO LOGADO VIA HTTPCONTEXT
// =====================================================

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();


// =====================================================
// POSTGRESQL
// =====================================================

builder.Services.AddDbContext<AppDbContext>(
    options =>
    {
        var connectionString =
            builder.Configuration
                .GetConnectionString(
                    "DefaultConnection"
                );

        options.UseNpgsql(
            connectionString
        );
    }
);


// =====================================================
// ASP.NET CORE IDENTITY
// =====================================================

builder.Services
    .AddIdentity<AppUser, IdentityRole>(
        options =>
        {
            // =============================================
            // SENHA
            // =============================================

            options.Password.RequiredLength = 6;

            options.Password.RequireDigit = true;

            options.Password.RequireLowercase = true;

            options.Password.RequireUppercase = true;

            options.Password.RequireNonAlphanumeric = false;


            // =============================================
            // USUÁRIO
            // =============================================

            options.User.RequireUniqueEmail = true;


            // =============================================
            // LOGIN
            // =============================================

            options.SignIn.RequireConfirmedEmail = false;

            options.SignIn.RequireConfirmedAccount = false;


            // =============================================
            // BLOQUEIO POR TENTATIVAS
            // =============================================

            options.Lockout.AllowedForNewUsers = true;

            options.Lockout.MaxFailedAccessAttempts = 5;

            options.Lockout.DefaultLockoutTimeSpan =
                TimeSpan.FromMinutes(10);
        }
    )
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


// =====================================================
// COOKIE DE LOGIN
// =====================================================

builder.Services.ConfigureApplicationCookie(
    options =>
    {
        options.LoginPath =
            "/Conta/Login";

        options.AccessDeniedPath =
            "/Conta/AcessoNegado";

        options.ExpireTimeSpan =
            TimeSpan.FromDays(30);

        options.SlidingExpiration =
            true;

        options.Cookie.Name =
            "ChefeIA.Auth";

        options.Cookie.HttpOnly =
            true;

        options.Cookie.IsEssential =
            true;
    }
);


// =====================================================
// RECEITAS
// =====================================================

builder.Services.AddScoped<
    IReceitaService,
    ReceitaService
>();


// =====================================================
// CONFIGURAÇÕES DO SITE
// =====================================================

builder.Services.AddScoped<
    ISiteSettingsService,
    SiteSettingsService
>();


// =====================================================
// PLANOS
// =====================================================

builder.Services.AddScoped<
    IPlanService,
    PlanService
>();


// =====================================================
// CONTROLE DE LIMITE DA IA
// =====================================================

builder.Services.AddScoped<
    IAiUsageLimitService,
    AiUsageLimitService
>();


// =====================================================
// IA
// =====================================================

builder.Services.AddHttpClient<
    IChefeIAService,
    ChefeIAService
>();


// =====================================================
// ASAAS - CONFIGURAÇÕES
// =====================================================

builder.Services.Configure<AsaasOptions>(
    builder.Configuration
        .GetSection("Asaas")
);


// =====================================================
// ASAAS - SERVIÇO
// =====================================================

builder.Services.AddHttpClient<
    IAsaasService,
    AsaasService
>();


// =====================================================
// CONSTRUIR APLICAÇÃO
// =====================================================

var app =
    builder.Build();


// =====================================================
// CRIAR ROLES E USUÁRIO ADMIN
// =====================================================

await IdentitySeed.InicializarAsync(
    app.Services
);


// =====================================================
// PIPELINE
// =====================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Home/Error"
    );

    app.UseHsts();
}


// =====================================================
// HTTPS
// =====================================================

app.UseHttpsRedirection();


// =====================================================
// ARQUIVOS ESTÁTICOS
// =====================================================

app.MapStaticAssets();


// =====================================================
// ROTAS
// =====================================================

app.UseRouting();


// =====================================================
// AUTENTICAÇÃO
// =====================================================

app.UseAuthentication();


// =====================================================
// AUTORIZAÇÃO
// =====================================================

app.UseAuthorization();


// =====================================================
// CONTROLLERS / API
// =====================================================

app.MapControllers();


// =====================================================
// ROTA MVC
// =====================================================

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/{action=Index}/{id?}"
)
.WithStaticAssets();


// =====================================================
// INICIAR
// =====================================================

app.Run();