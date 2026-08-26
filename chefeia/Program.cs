using chefeia.Data;
using chefeia.Services;
using chefeia.Services.AI;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =====================================================
// SERVIÇOS
// =====================================================

// Controllers + Views MVC
builder.Services.AddControllersWithViews();


// =====================================================
// BANCO DE DADOS - POSTGRESQL
// =====================================================

// AppDbContext será responsável pela comunicação
// entre o Chefe IA e o PostgreSQL.
//
// Futuramente teremos aqui:
// - Usuários
// - Planos
// - Assinaturas
// - Consumo de IA
// - Histórico de consultas
// - Configurações administrativas
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString =
        builder.Configuration.GetConnectionString(
            "DefaultConnection"
        );

    options.UseNpgsql(connectionString);
});


// =====================================================
// SERVIÇO DE RECEITAS
// =====================================================

// Por enquanto utiliza nossas receitas de teste.
// Mais adiante as receitas poderão vir do PostgreSQL.
builder.Services.AddScoped<IReceitaService, ReceitaService>();


// =====================================================
// SERVIÇO DO CHEFE IA
// =====================================================

// HttpClient permite que o ChefeIAService
// faça chamadas para a API externa de IA.
builder.Services.AddHttpClient<IChefeIAService, ChefeIAService>();


// =====================================================
// CONSTRUÇÃO DA APLICAÇÃO
// =====================================================

var app = builder.Build();


// =====================================================
// PIPELINE HTTP
// =====================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}


// Redireciona HTTP para HTTPS
app.UseHttpsRedirection();


// =====================================================
// ARQUIVOS ESTÁTICOS
// =====================================================

// CSS
// JavaScript
// Imagens
app.MapStaticAssets();


// =====================================================
// ROTAS
// =====================================================

app.UseRouting();


// =====================================================
// AUTORIZAÇÃO
// =====================================================

// Mais adiante adicionaremos:
//
// Login
// Cadastro
// Usuário administrador
// Plano Gratuito
// Plano Premium
// Controle de acesso ao painel administrativo
app.UseAuthorization();


// =====================================================
// ENDPOINTS DA API
// =====================================================

// Exemplos:
//
// GET  /api/receitas
// GET  /api/receitas/buscar
// GET  /api/ingredientes/buscar
// GET  /api/paises
//
// POST /api/ai/sugerir-receita

app.MapControllers();


// =====================================================
// ROTA MVC
// =====================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


// =====================================================
// INICIAR APLICAÇÃO
// =====================================================

app.Run();