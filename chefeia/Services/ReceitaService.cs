using chefeia.Data;
using chefeia.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace chefeia.Services
{
    public class ReceitaService : IReceitaService
    {
        private readonly AppDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<AppUser> _userManager;


        // =====================================================
        // RECEITAS FIXAS DO CATÁLOGO
        // =====================================================

        private readonly List<Receita> _receitas =
        [
            new Receita
            {
                Id = 1,

                Nome = "Brigadeiro",

                Descricao =
                    "Clássico doce brasileiro feito com leite condensado e chocolate.",

                Pais = "Brasil",

                Bandeira = "🇧🇷",

                Categoria = "Doces",

                ImagemUrl =
                    "https://images.unsplash.com/photo-1578985545062-69928b1d9587",

                TempoPreparoMinutos = 25,

                Dificuldade = "Fácil",

                Porcoes = 20,

                CriadaPorIa = false,

                Ingredientes =
                [
                    "1 lata de leite condensado",
                    "1 colher de sopa de manteiga",
                    "3 colheres de sopa de chocolate em pó",
                    "Chocolate granulado para finalizar"
                ],

                Passos =
                [
                    "Coloque o leite condensado, a manteiga e o chocolate em pó em uma panela.",
                    "Leve ao fogo baixo e mexa sem parar.",
                    "Continue mexendo até a mistura desgrudar do fundo da panela.",
                    "Desligue o fogo e deixe esfriar.",
                    "Faça pequenas bolinhas com as mãos untadas.",
                    "Passe as bolinhas no chocolate granulado e sirva."
                ]
            },


            new Receita
            {
                Id = 2,

                Nome = "Tiramisu",

                Descricao =
                    "Sobremesa italiana cremosa preparada com café e camadas delicadas.",

                Pais = "Itália",

                Bandeira = "🇮🇹",

                Categoria = "Sobremesas",

                ImagemUrl =
                    "https://images.unsplash.com/photo-1571877227200-a0d98ea607e9",

                TempoPreparoMinutos = 40,

                Dificuldade = "Médio",

                Porcoes = 8,

                CriadaPorIa = false,

                Ingredientes =
                [
                    "300 g de biscoito tipo champagne",
                    "500 g de queijo mascarpone",
                    "3 ovos",
                    "100 g de açúcar",
                    "300 ml de café forte frio",
                    "Cacau em pó para finalizar"
                ],

                Passos =
                [
                    "Separe as claras das gemas.",
                    "Bata as gemas com o açúcar até obter um creme claro.",
                    "Misture o mascarpone ao creme.",
                    "Bata as claras em neve e incorpore delicadamente.",
                    "Molhe rapidamente os biscoitos no café frio.",
                    "Monte camadas de biscoito e creme.",
                    "Finalize com cacau em pó.",
                    "Leve à geladeira antes de servir."
                ]
            },


            new Receita
            {
                Id = 3,

                Nome = "Pastel",

                Descricao =
                    "Massa crocante recheada, muito popular no Brasil.",

                Pais = "Brasil",

                Bandeira = "🇧🇷",

                Categoria = "Salgados",

                ImagemUrl =
                    "https://images.unsplash.com/photo-1626132647523-66f5bf380027",

                TempoPreparoMinutos = 35,

                Dificuldade = "Fácil",

                Porcoes = 8,

                CriadaPorIa = false,

                Ingredientes =
                [
                    "500 g de massa para pastel",
                    "300 g de carne moída ou recheio de sua preferência",
                    "1 cebola pequena picada",
                    "Sal a gosto",
                    "Pimenta-do-reino a gosto",
                    "Óleo para fritar"
                ],

                Passos =
                [
                    "Prepare o recheio e deixe esfriar.",
                    "Abra a massa para pastel.",
                    "Coloque uma pequena quantidade de recheio no centro.",
                    "Dobre a massa e feche as bordas com um garfo.",
                    "Aqueça o óleo.",
                    "Frite os pastéis até ficarem dourados.",
                    "Escorra em papel-toalha e sirva."
                ]
            }
        ];


        // =====================================================
        // CONSTRUTOR
        // =====================================================

        public ReceitaService(
            AppDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            UserManager<AppUser> userManager)
        {
            _dbContext =
                dbContext;

            _httpContextAccessor =
                httpContextAccessor;

            _userManager =
                userManager;
        }


        // =====================================================
        // DESTAQUES
        // =====================================================

        public IEnumerable<Receita> ObterDestaques()
        {
            return _receitas;
        }


        // =====================================================
        // BUSCAR
        // =====================================================

        public IEnumerable<Receita> Buscar(
            string termo)
        {
            var resultado =
                new List<Receita>();


            // =================================================
            // RECEITAS FIXAS
            // =================================================

            if (string.IsNullOrWhiteSpace(
                termo))
            {
                resultado.AddRange(
                    _receitas
                );
            }
            else
            {
                var termoLimpo =
                    termo.Trim();


                resultado.AddRange(
                    _receitas.Where(
                        r =>
                            Contem(
                                r.Nome,
                                termoLimpo
                            ) ||

                            Contem(
                                r.Descricao,
                                termoLimpo
                            ) ||

                            Contem(
                                r.Categoria,
                                termoLimpo
                            ) ||

                            Contem(
                                r.Pais,
                                termoLimpo
                            ) ||

                            r.Ingredientes.Any(
                                x =>
                                    Contem(
                                        x,
                                        termoLimpo
                                    )
                            )
                    )
                );
            }


            // =================================================
            // USUÁRIO LOGADO
            // =================================================

            var httpContext =
                _httpContextAccessor
                    .HttpContext;


            if (
                httpContext == null ||
                httpContext.User.Identity
                    ?.IsAuthenticated != true)
            {
                return resultado;
            }


            var userId =
                _userManager
                    .GetUserId(
                        httpContext.User
                    );


            if (string.IsNullOrWhiteSpace(
                userId))
            {
                return resultado;
            }


            // =================================================
            // RECEITAS GERADAS PELA IA
            // =================================================

            var query =
                _dbContext
                    .RecipeHistories
                    .AsNoTracking()
                    .Where(
                        x =>
                            x.UserId ==
                            userId
                    );


            if (!string.IsNullOrWhiteSpace(
                termo))
            {
                var termoBusca =
                    termo
                        .Trim()
                        .ToLower();


                query =
                    query.Where(
                        x =>
                            x.Name
                                .ToLower()
                                .Contains(
                                    termoBusca
                                ) ||

                            x.Description
                                .ToLower()
                                .Contains(
                                    termoBusca
                                ) ||

                            x.Category
                                .ToLower()
                                .Contains(
                                    termoBusca
                                ) ||

                            x.Country
                                .ToLower()
                                .Contains(
                                    termoBusca
                                ) ||

                            x.RequestedIngredients
                                .ToLower()
                                .Contains(
                                    termoBusca
                                )
                    );
            }


            var receitasGeradas =
                query
                    .OrderByDescending(
                        x => x.CreatedAt
                    )
                    .Take(50)
                    .ToList();


            // =================================================
            // CONVERTER HISTÓRICO PARA RECEITA
            // =================================================

            foreach (
                var item
                in receitasGeradas)
            {
                var ingredientes =
                    DeserializarLista(
                        item.IngredientsJson
                    );


                var passos =
                    DeserializarLista(
                        item.StepsJson
                    );


                resultado.Add(
                    new Receita
                    {
                        Id =
                            1000000 +
                            item.Id,

                        Nome =
                            item.Name,

                        Descricao =
                            item.Description,

                        Pais =
                            string.IsNullOrWhiteSpace(
                                item.Country)
                                ? "Chefe IA"
                                : item.Country,

                        Bandeira =
                            ObterBandeira(
                                item.Country
                            ),

                        Categoria =
                            string.IsNullOrWhiteSpace(
                                item.Category)
                                ? "Receita IA"
                                : item.Category,

                        // =================================================
                        // SEM IMAGEM FALSA
                        //
                        // Vamos deixar vazio.
                        // No próximo passo o card vai detectar isso
                        // e remover completamente a área da imagem.
                        // =================================================

                        ImagemUrl =
                            string.Empty,

                        TempoPreparoMinutos =
                            item.PreparationMinutes,

                        Dificuldade =
                            "Criada pela IA",

                        Porcoes =
                            item.Servings,

                        Ingredientes =
                            ingredientes,

                        Passos =
                            passos,

                        CriadaPorIa =
                            true
                    }
                );
            }


            // =================================================
            // EVITAR DUPLICADOS
            // =================================================

            return resultado
                .GroupBy(
                    x =>
                        x.Nome.Trim(),
                    StringComparer
                        .OrdinalIgnoreCase
                )
                .Select(
                    x => x.First()
                )
                .ToList();
        }


        // =====================================================
        // DESERIALIZAR LISTA
        // =====================================================

        private static List<string> DeserializarLista(
            string? json)
        {
            if (string.IsNullOrWhiteSpace(
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
        // VERIFICAR TEXTO
        // =====================================================

        private static bool Contem(
            string? texto,
            string termo)
        {
            if (string.IsNullOrWhiteSpace(
                texto))
            {
                return false;
            }


            return texto.Contains(
                termo,
                StringComparison
                    .OrdinalIgnoreCase
            );
        }


        // =====================================================
        // BANDEIRA
        // =====================================================

        private static string ObterBandeira(
            string? pais)
        {
            if (string.IsNullOrWhiteSpace(
                pais))
            {
                return "👨‍🍳";
            }


            var valor =
                pais
                    .Trim()
                    .ToLowerInvariant();


            if (
                valor.Contains("brasil") ||
                valor.Contains("brasile"))
            {
                return "🇧🇷";
            }


            if (
                valor.Contains("ital"))
            {
                return "🇮🇹";
            }


            if (
                valor.Contains("fran"))
            {
                return "🇫🇷";
            }


            if (
                valor.Contains("méxico") ||
                valor.Contains("mexico"))
            {
                return "🇲🇽";
            }


            if (
                valor.Contains("jap"))
            {
                return "🇯🇵";
            }


            if (
                valor.Contains("china"))
            {
                return "🇨🇳";
            }


            if (
                valor.Contains("espan"))
            {
                return "🇪🇸";
            }


            if (
                valor.Contains("portugal"))
            {
                return "🇵🇹";
            }


            if (
                valor.Contains("argentin"))
            {
                return "🇦🇷";
            }


            if (
                valor.Contains("estados unidos") ||
                valor.Contains("americana") ||
                valor.Contains("americano"))
            {
                return "🇺🇸";
            }


            return "🌎";
        }
    }
}