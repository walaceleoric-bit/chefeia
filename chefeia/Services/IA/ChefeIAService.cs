using chefeia.Data;
using chefeia.Models;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace chefeia.Services.AI
{
    public class ChefeIAService : IChefeIAService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChefeIAService> _logger;
        private readonly AppDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<AppUser> _userManager;


        public ChefeIAService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ChefeIAService> logger,
            AppDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            UserManager<AppUser> userManager)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }


        // =========================================================
        // SUGERIR RECEITA
        // =========================================================

        public async Task<ReceitaIA> SugerirReceitaAsync(
            ConsultaReceitaIA consulta)
        {
            // =====================================================
            // CONFIGURAÇÕES
            // =====================================================

            var apiKey =
                _configuration["RapidApi:Key"];

            var apiHost =
                _configuration["RapidApi:Host"];

            var apiUrl =
                _configuration["RapidApi:Url"];

            var model =
                _configuration["RapidApi:Model"];


            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "A chave da RapidAPI não foi configurada.");
            }


            if (string.IsNullOrWhiteSpace(apiHost))
            {
                throw new InvalidOperationException(
                    "O Host da RapidAPI não foi configurado.");
            }


            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                throw new InvalidOperationException(
                    "A URL da RapidAPI não foi configurada.");
            }


            if (string.IsNullOrWhiteSpace(model))
            {
                model =
                    "gpt-5";
            }


            // =====================================================
            // VALIDAR CONSULTA
            // =====================================================

            if (
                consulta.Ingredientes == null ||
                consulta.Ingredientes.Count == 0)
            {
                throw new ArgumentException(
                    "Informe pelo menos um ingrediente.");
            }


            var ingredientesLimpos =
                consulta.Ingredientes
                    .Where(
                        x => !string.IsNullOrWhiteSpace(x))
                    .Select(
                        x => x.Trim())
                    .Distinct(
                        StringComparer.OrdinalIgnoreCase)
                    .ToList();


            if (ingredientesLimpos.Count == 0)
            {
                throw new ArgumentException(
                    "Informe pelo menos um ingrediente válido.");
            }


            var ingredientes =
                string.Join(
                    ", ",
                    ingredientesLimpos);


            var preferencia =
                string.IsNullOrWhiteSpace(
                    consulta.Preferencia)
                    ? "qualquer"
                    : consulta.Preferencia.Trim();


            var porcoes =
                consulta.Porcoes > 0
                    ? consulta.Porcoes
                    : 1;


            // =====================================================
            // IDENTIFICAR USUÁRIO
            // =====================================================

            var httpContext =
                _httpContextAccessor.HttpContext;


            if (httpContext == null)
            {
                throw new UnauthorizedAccessException(
                    "Não foi possível identificar o usuário.");
            }


            var usuario =
                await _userManager
                    .GetUserAsync(
                        httpContext.User);


            if (usuario == null)
            {
                throw new UnauthorizedAccessException(
                    "Faça login para utilizar o Chefe IA.");
            }


            if (!usuario.IsActive)
            {
                throw new UnauthorizedAccessException(
                    "Esta conta está desativada.");
            }


            // =====================================================
            // PLANO
            // =====================================================

            var planCode =
                string.IsNullOrWhiteSpace(
                    usuario.PlanCode)
                    ? "FREE"
                    : usuario.PlanCode
                        .Trim()
                        .ToUpperInvariant();


            if (
                planCode != "FREE" &&
                planCode != "PREMIUM")
            {
                planCode =
                    "FREE";
            }


            // =====================================================
            // REGISTRO DE CONSUMO
            // =====================================================

            var consumo =
                new AiUsage
                {
                    CreatedAt =
                        DateTime.UtcNow,

                    Success =
                        false,

                    IngredientCount =
                        ingredientesLimpos.Count,

                    Servings =
                        porcoes,

                    Preference =
                        preferencia,

                    UserId =
                        usuario.Id,

                    PlanName =
                        planCode
                };


            // =====================================================
            // PROMPT
            // =====================================================

            var prompt =
                MontarPrompt(
                    ingredientes,
                    preferencia,
                    porcoes);


            // =====================================================
            // BODY
            // =====================================================

            var corpoRequisicao =
                new
                {
                    model = model,

                    messages =
                        new[]
                        {
                            new
                            {
                                role = "user",
                                content = prompt
                            }
                        }
                };


            var jsonRequisicao =
                JsonSerializer.Serialize(
                    corpoRequisicao);


            // =====================================================
            // REQUEST
            // =====================================================

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    apiUrl);


            request.Headers.Add(
                "x-rapidapi-key",
                apiKey);


            request.Headers.Add(
                "x-rapidapi-host",
                apiHost);


            request.Content =
                new StringContent(
                    jsonRequisicao,
                    Encoding.UTF8,
                    "application/json");


            var cronometro =
                Stopwatch.StartNew();


            HttpResponseMessage? response =
                null;


            try
            {
                // =================================================
                // CHAMAR IA
                // =================================================

                response =
                    await _httpClient
                        .SendAsync(request);


                cronometro.Stop();


                var conteudoResposta =
                    await response.Content
                        .ReadAsStringAsync();

                _logger.LogInformation(
    "RESPOSTA BRUTA DA RAPIDAPI: {Resposta}",
    conteudoResposta);


                consumo.StatusCode =
                    (int)response.StatusCode;


                consumo.DurationMs =
                    cronometro.ElapsedMilliseconds;


                consumo.RequestsLimit =
                    ObterHeaderInt(
                        response,
                        "X-RateLimit-Requests-Limit");


                consumo.RequestsRemaining =
                    ObterHeaderInt(
                        response,
                        "X-RateLimit-Requests-Remaining");


                consumo.CreditLimit =
                    ObterHeaderInt(
                        response,
                        "X-RateLimit-Credit-Limit");


                consumo.CreditRemaining =
                    ObterHeaderInt(
                        response,
                        "X-RateLimit-Credit-Remaining");


                // =================================================
                // LIMITE DA API EXTERNA
                // =================================================

                if (
                    response.StatusCode ==
                    System.Net.HttpStatusCode.TooManyRequests)
                {
                    consumo.Success =
                        false;


                    consumo.ErrorMessage =
                        LimitarTexto(
                            "RapidAPI 429: " +
                            conteudoResposta,
                            2000);


                    await SalvarConsumoAsync(
                        consumo);


                    throw new InvalidOperationException(
                        "O serviço de inteligência artificial atingiu temporariamente o limite de uso. Tente novamente mais tarde.");
                }


                // =================================================
                // OUTROS ERROS HTTP
                // =================================================

                if (!response.IsSuccessStatusCode)
                {
                    consumo.Success =
                        false;


                    consumo.ErrorMessage =
                        LimitarTexto(
                            "HTTP " +
                            (int)response.StatusCode +
                            ": " +
                            conteudoResposta,
                            2000);


                    await SalvarConsumoAsync(
                        consumo);


                    throw new HttpRequestException(
                        "Não foi possível consultar o serviço de inteligência artificial.");
                }


                // =================================================
                // LER RESPOSTA
                // choices[0].message.content
                // =================================================

                using var documentoApi =
                    JsonDocument.Parse(
                        conteudoResposta);


                var raizApi =
                    documentoApi.RootElement;


                if (
                    !TryGetPropertyIgnoreCase(
                        raizApi,
                        "choices",
                        out var choicesElement) ||
                    choicesElement.ValueKind !=
                        JsonValueKind.Array ||
                    choicesElement.GetArrayLength() == 0)
                {
                    throw new InvalidOperationException(
                        "A API não retornou nenhuma resposta.");
                }


                var primeiraEscolha =
                    choicesElement[0];


                if (
                    !TryGetPropertyIgnoreCase(
                        primeiraEscolha,
                        "message",
                        out var messageElement))
                {
                    throw new InvalidOperationException(
                        "A API não retornou a mensagem da inteligência artificial.");
                }


                if (
                    !TryGetPropertyIgnoreCase(
                        messageElement,
                        "content",
                        out var contentElement))
                {
                    throw new InvalidOperationException(
                        "A API não retornou o conteúdo da resposta.");
                }


                var resultadoIA =
                    contentElement.GetString();


                if (string.IsNullOrWhiteSpace(resultadoIA))
                {
                    throw new InvalidOperationException(
                        "A inteligência artificial retornou uma resposta vazia.");
                }


                resultadoIA =
                    LimparJson(
                        resultadoIA);


                _logger.LogInformation(
                    "JSON recebido da IA: {Json}",
                    resultadoIA);


                // =================================================
                // CONVERTER RESPOSTA FLEXÍVEL
                // =================================================

                ReceitaIA receita;


                try
                {
                    receita =
                        ConverterRespostaIA(
                            resultadoIA);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "JSON retornado pela IA: {Json}",
                        resultadoIA);


                    throw new InvalidOperationException(
                        "A inteligência artificial respondeu em um formato inválido.",
                        ex);
                }


                // =================================================
                // NORMALIZAR
                // =================================================

                receita.TipoResposta =
                    NormalizarTipoResposta(
                        receita.TipoResposta);


                receita.Mensagem =
                    receita.Mensagem?.Trim()
                    ?? string.Empty;


                receita.Sugestoes ??=
                    new List<string>();


                receita.Sugestoes =
                    receita.Sugestoes
                        .Where(
                            x => !string.IsNullOrWhiteSpace(x))
                        .Select(
                            x => x.Trim())
                        .Distinct(
                            StringComparer.OrdinalIgnoreCase)
                        .Take(5)
                        .ToList();


                receita.Ingredientes ??=
                    new List<string>();


                receita.Passos ??=
                    new List<string>();


                // =================================================
                // VALIDAR RECEITA
                // =================================================

                if (receita.TemReceita)
                {
                    if (receita.Porcoes <= 0)
                    {
                        receita.Porcoes =
                            porcoes;
                    }


                    if (
                        string.IsNullOrWhiteSpace(
                            receita.Nome) ||
                        receita.Ingredientes.Count == 0 ||
                        receita.Passos.Count == 0)
                    {
                        receita.TipoResposta =
                            "INSUFICIENTE";


                        receita.Mensagem =
                            "Não encontrei uma receita completa e confiável com esses ingredientes.";


                        receita.Sugestoes =
                            new List<string>
                            {
                                "Adicione mais algum ingrediente e tente novamente."
                            };


                        LimparCamposReceita(
                            receita);
                    }
                }
                else
                {
                    LimparCamposReceita(
                        receita);


                    if (
                        string.IsNullOrWhiteSpace(
                            receita.Mensagem))
                    {
                        receita.Mensagem =
                            receita.TipoResposta ==
                            "SUGESTAO"
                                ? "Com esses ingredientes as opções são limitadas. Vale acrescentar mais algum item."
                                : "Ainda não encontrei uma combinação culinária que eu recomende.";
                    }


                    if (
                        receita.Sugestoes.Count == 0)
                    {
                        receita.Sugestoes.Add(
                            "Adicione mais um ingrediente que você tenha em casa e tente novamente.");
                    }
                }


                // =================================================
                // CONSULTA VÁLIDA
                // =================================================

                consumo.Success =
                    true;


                consumo.ErrorMessage =
                    null;


                await SalvarConsumoAsync(
                    consumo);


                // =================================================
                // HISTÓRICO PREMIUM
                // =================================================

                if (
                    planCode == "PREMIUM" &&
                    receita.TemReceita)
                {
                    await SalvarHistoricoAsync(
                        usuario,
                        receita,
                        consulta,
                        preferencia);
                }


                return receita;
            }
            catch (Exception ex)
            {
                if (cronometro.IsRunning)
                {
                    cronometro.Stop();
                }


                if (consumo.Id == 0)
                {
                    consumo.Success =
                        false;


                    consumo.DurationMs =
                        cronometro.ElapsedMilliseconds;


                    consumo.ErrorMessage =
                        LimitarTexto(
                            ex.Message,
                            2000);


                    if (response != null)
                    {
                        consumo.StatusCode =
                            (int)response.StatusCode;
                    }


                    await SalvarConsumoAsync(
                        consumo);
                }


                _logger.LogError(
                    ex,
                    "Falha na consulta do Chefe IA.");


                throw;
            }
            finally
            {
                response?.Dispose();
            }
        }


        // =========================================================
        // CONVERTER RESPOSTA DA IA
        // =========================================================

        private static ReceitaIA ConverterRespostaIA(
            string json)
        {
            using var documento =
                JsonDocument.Parse(
                    json);


            var raiz =
                documento.RootElement;


            var receita =
                new ReceitaIA
                {
                    TipoResposta =
                        ObterTexto(
                            raiz,
                            "tipoResposta"),

                    Mensagem =
                        ObterTexto(
                            raiz,
                            "mensagem"),

                    Nome =
                        ObterTexto(
                            raiz,
                            "nome"),

                    Descricao =
                        ObterTexto(
                            raiz,
                            "descricao"),

                    Pais =
                        ObterTexto(
                            raiz,
                            "pais"),

                    Categoria =
                        ObterTexto(
                            raiz,
                            "categoria"),

                    Porcoes =
                        ObterInteiro(
                            raiz,
                            "porcoes"),

                    TempoMinutos =
                        ObterInteiro(
                            raiz,
                            "tempoMinutos"),

                    Sugestoes =
                        ObterListaFlexivel(
                            raiz,
                            "sugestoes"),

                    Ingredientes =
                        ObterIngredientes(
                            raiz),

                    Passos =
                        ObterPassos(
                            raiz)
                };


            return receita;
        }


        // =========================================================
        // INGREDIENTES
        // =========================================================

        private static List<string> ObterIngredientes(
            JsonElement raiz)
        {
            var resultado =
                new List<string>();


            if (
                !TryGetPropertyIgnoreCase(
                    raiz,
                    "ingredientes",
                    out var elemento))
            {
                return resultado;
            }


            if (
                elemento.ValueKind !=
                JsonValueKind.Array)
            {
                return resultado;
            }


            foreach (var item in elemento.EnumerateArray())
            {
                if (
                    item.ValueKind ==
                    JsonValueKind.String)
                {
                    var texto =
                        item.GetString();


                    if (!string.IsNullOrWhiteSpace(texto))
                    {
                        resultado.Add(
                            texto.Trim());
                    }


                    continue;
                }


                if (
                    item.ValueKind ==
                    JsonValueKind.Object)
                {
                    var quantidade =
                        PrimeiroTextoDisponivel(
                            item,
                            "quantidade",
                            "quantity",
                            "amount",
                            "qtd");


                    var nome =
                        PrimeiroTextoDisponivel(
                            item,
                            "nome",
                            "name",
                            "ingrediente",
                            "ingredient",
                            "item");


                    var unidade =
                        PrimeiroTextoDisponivel(
                            item,
                            "unidade",
                            "unit");


                    var partes =
                        new List<string>();


                    if (!string.IsNullOrWhiteSpace(quantidade))
                    {
                        partes.Add(
                            quantidade);
                    }


                    if (!string.IsNullOrWhiteSpace(unidade))
                    {
                        partes.Add(
                            unidade);
                    }


                    if (!string.IsNullOrWhiteSpace(nome))
                    {
                        partes.Add(
                            nome);
                    }


                    var textoFinal =
                        string.Join(
                            " ",
                            partes)
                        .Trim();


                    if (string.IsNullOrWhiteSpace(textoFinal))
                    {
                        textoFinal =
                            ObterPrimeiroValorTexto(
                                item);
                    }


                    if (!string.IsNullOrWhiteSpace(textoFinal))
                    {
                        resultado.Add(
                            textoFinal);
                    }
                }
            }


            return resultado;
        }


        // =========================================================
        // PASSOS
        // =========================================================

        private static List<string> ObterPassos(
            JsonElement raiz)
        {
            var resultado =
                new List<string>();


            if (
                !TryGetPropertyIgnoreCase(
                    raiz,
                    "passos",
                    out var elemento))
            {
                return resultado;
            }


            if (
                elemento.ValueKind !=
                JsonValueKind.Array)
            {
                return resultado;
            }


            foreach (var item in elemento.EnumerateArray())
            {
                if (
                    item.ValueKind ==
                    JsonValueKind.String)
                {
                    var texto =
                        item.GetString();


                    if (!string.IsNullOrWhiteSpace(texto))
                    {
                        resultado.Add(
                            texto.Trim());
                    }


                    continue;
                }


                if (
                    item.ValueKind ==
                    JsonValueKind.Object)
                {
                    var texto =
                        PrimeiroTextoDisponivel(
                            item,
                            "descricao",
                            "descrição",
                            "texto",
                            "instrucao",
                            "instrução",
                            "instruction",
                            "step",
                            "passo");


                    if (string.IsNullOrWhiteSpace(texto))
                    {
                        texto =
                            ObterPrimeiroValorTexto(
                                item);
                    }


                    if (!string.IsNullOrWhiteSpace(texto))
                    {
                        resultado.Add(
                            texto);
                    }
                }
            }


            return resultado;
        }


        // =========================================================
        // LISTA FLEXÍVEL
        // =========================================================

        private static List<string> ObterListaFlexivel(
            JsonElement raiz,
            string propriedade)
        {
            var resultado =
                new List<string>();


            if (
                !TryGetPropertyIgnoreCase(
                    raiz,
                    propriedade,
                    out var elemento))
            {
                return resultado;
            }


            if (
                elemento.ValueKind !=
                JsonValueKind.Array)
            {
                return resultado;
            }


            foreach (var item in elemento.EnumerateArray())
            {
                if (
                    item.ValueKind ==
                    JsonValueKind.String)
                {
                    var texto =
                        item.GetString();


                    if (!string.IsNullOrWhiteSpace(texto))
                    {
                        resultado.Add(
                            texto.Trim());
                    }
                }
                else if (
                    item.ValueKind ==
                    JsonValueKind.Object)
                {
                    var texto =
                        ObterPrimeiroValorTexto(
                            item);


                    if (!string.IsNullOrWhiteSpace(texto))
                    {
                        resultado.Add(
                            texto);
                    }
                }
            }


            return resultado;
        }


        // =========================================================
        // OBTER TEXTO
        // =========================================================

        private static string ObterTexto(
            JsonElement raiz,
            string propriedade)
        {
            if (
                !TryGetPropertyIgnoreCase(
                    raiz,
                    propriedade,
                    out var elemento))
            {
                return string.Empty;
            }


            if (
                elemento.ValueKind ==
                JsonValueKind.String)
            {
                return elemento.GetString()?.Trim()
                    ?? string.Empty;
            }


            return elemento.ToString().Trim();
        }


        // =========================================================
        // OBTER INTEIRO
        // =========================================================

        private static int ObterInteiro(
            JsonElement raiz,
            string propriedade)
        {
            if (
                !TryGetPropertyIgnoreCase(
                    raiz,
                    propriedade,
                    out var elemento))
            {
                return 0;
            }


            if (
                elemento.ValueKind ==
                JsonValueKind.Number &&
                elemento.TryGetInt32(
                    out var numero))
            {
                return numero;
            }


            if (
                elemento.ValueKind ==
                JsonValueKind.String &&
                int.TryParse(
                    elemento.GetString(),
                    out numero))
            {
                return numero;
            }


            return 0;
        }


        // =========================================================
        // PROPRIEDADE IGNORANDO MAIÚSCULAS
        // =========================================================

        private static bool TryGetPropertyIgnoreCase(
            JsonElement elemento,
            string nome,
            out JsonElement valor)
        {
            if (
                elemento.ValueKind ==
                JsonValueKind.Object)
            {
                foreach (
                    var propriedade
                    in elemento.EnumerateObject())
                {
                    if (
                        string.Equals(
                            propriedade.Name,
                            nome,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        valor =
                            propriedade.Value;

                        return true;
                    }
                }
            }


            valor =
                default;

            return false;
        }


        // =========================================================
        // PRIMEIRO TEXTO DISPONÍVEL
        // =========================================================

        private static string PrimeiroTextoDisponivel(
            JsonElement elemento,
            params string[] nomes)
        {
            foreach (var nome in nomes)
            {
                if (
                    TryGetPropertyIgnoreCase(
                        elemento,
                        nome,
                        out var valor))
                {
                    if (
                        valor.ValueKind ==
                        JsonValueKind.String)
                    {
                        var texto =
                            valor.GetString();


                        if (!string.IsNullOrWhiteSpace(texto))
                        {
                            return texto.Trim();
                        }
                    }


                    if (
                        valor.ValueKind ==
                        JsonValueKind.Number)
                    {
                        return valor.ToString();
                    }
                }
            }


            return string.Empty;
        }


        // =========================================================
        // PRIMEIRO VALOR TEXTUAL
        // =========================================================

        private static string ObterPrimeiroValorTexto(
            JsonElement elemento)
        {
            if (
                elemento.ValueKind !=
                JsonValueKind.Object)
            {
                return string.Empty;
            }


            foreach (
                var propriedade
                in elemento.EnumerateObject())
            {
                if (
                    propriedade.Value.ValueKind ==
                    JsonValueKind.String)
                {
                    var texto =
                        propriedade.Value
                            .GetString();


                    if (!string.IsNullOrWhiteSpace(texto))
                    {
                        return texto.Trim();
                    }
                }
            }


            return string.Empty;
        }


        // =========================================================
        // NORMALIZAR TIPO
        // =========================================================

        private static string NormalizarTipoResposta(
            string? tipoResposta)
        {
            var tipo =
                (tipoResposta ?? string.Empty)
                    .Trim()
                    .ToUpperInvariant();


            if (tipo == "RECEITA")
            {
                return "RECEITA";
            }


            if (
                tipo == "SUGESTAO" ||
                tipo == "SUGESTÃO")
            {
                return "SUGESTAO";
            }


            if (tipo == "INSUFICIENTE")
            {
                return "INSUFICIENTE";
            }


            return "INSUFICIENTE";
        }


        // =========================================================
        // LIMPAR CAMPOS
        // =========================================================

        private static void LimparCamposReceita(
            ReceitaIA receita)
        {
            receita.Nome =
                string.Empty;


            receita.Descricao =
                string.Empty;


            receita.Pais =
                string.Empty;


            receita.Categoria =
                string.Empty;


            receita.Porcoes =
                0;


            receita.TempoMinutos =
                0;


            receita.Ingredientes =
                new List<string>();


            receita.Passos =
                new List<string>();
        }


        // =========================================================
        // SALVAR CONSUMO
        // =========================================================

        private async Task SalvarConsumoAsync(
            AiUsage consumo)
        {
            try
            {
                _dbContext.AiUsages.Add(
                    consumo);


                await _dbContext
                    .SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Não foi possível registrar o consumo da IA no banco.");
            }
        }


        // =========================================================
        // HISTÓRICO PREMIUM
        // =========================================================

        private async Task SalvarHistoricoAsync(
            AppUser usuario,
            ReceitaIA receita,
            ConsultaReceitaIA consulta,
            string preferencia)
        {
            try
            {
                var historico =
                    new RecipeHistory
                    {
                        UserId =
                            usuario.Id,

                        Name =
                            receita.Nome,

                        Country =
                            receita.Pais,

                        Category =
                            receita.Categoria,

                        Description =
                            receita.Descricao,

                        PreparationMinutes =
                            receita.TempoMinutos,

                        Servings =
                            receita.Porcoes,

                        IngredientsJson =
                            JsonSerializer.Serialize(
                                receita.Ingredientes),

                        StepsJson =
                            JsonSerializer.Serialize(
                                receita.Passos),

                        RequestedIngredients =
                            LimitarTexto(
                                string.Join(
                                    ", ",
                                    consulta.Ingredientes),
                                1000),

                        Preference =
                            LimitarTexto(
                                preferencia,
                                150),

                        CreatedAt =
                            DateTime.UtcNow
                    };


                _dbContext.RecipeHistories.Add(
                    historico);


                await _dbContext
                    .SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Não foi possível salvar a receita no histórico Premium.");
            }
        }


        // =========================================================
        // HEADER
        // =========================================================

        private static int? ObterHeaderInt(
            HttpResponseMessage response,
            string nomeHeader)
        {
            if (
                !response.Headers.TryGetValues(
                    nomeHeader,
                    out var valores))
            {
                return null;
            }


            if (
                int.TryParse(
                    valores.FirstOrDefault(),
                    out var numero))
            {
                return numero;
            }


            return null;
        }


        // =========================================================
        // LIMITAR TEXTO
        // =========================================================

        private static string LimitarTexto(
            string texto,
            int tamanhoMaximo)
        {
            if (string.IsNullOrEmpty(texto))
            {
                return string.Empty;
            }


            return texto.Length <= tamanhoMaximo
                ? texto
                : texto.Substring(
                    0,
                    tamanhoMaximo);
        }


        // =========================================================
        // PROMPT
        // =========================================================

        private static string MontarPrompt(
            string ingredientes,
            string preferencia,
            int porcoes)
        {
            var prompt =
                new StringBuilder();


            prompt.AppendLine(
                "Você é o Chefe IA, um chef virtual criterioso, responsável e exigente com coerência culinária.");

            prompt.AppendLine();

            prompt.AppendLine(
                "INGREDIENTES INFORMADOS PELO USUÁRIO:");

            prompt.AppendLine(
                ingredientes);

            prompt.AppendLine();

            prompt.AppendLine(
                "PREFERÊNCIA:");

            prompt.AppendLine(
                preferencia);

            prompt.AppendLine();

            prompt.AppendLine(
                "PORÇÕES:");

            prompt.AppendLine(
                porcoes.ToString());

            prompt.AppendLine();

            prompt.AppendLine(
                "Antes de gerar qualquer receita, avalie se TODOS os ingredientes informados podem participar de uma preparação coerente.");

            prompt.AppendLine();

            prompt.AppendLine(
                "REGRAS OBRIGATÓRIAS:");

            prompt.AppendLine(
                "1. Não crie uma receita apenas para responder ao usuário.");

            prompt.AppendLine(
                "2. Use todos os ingredientes informados pelo usuário sempre que houver uma forma culinariamente coerente de utilizá-los.");

            prompt.AppendLine(
                "3. Não ignore silenciosamente nenhum ingrediente informado.");

            prompt.AppendLine(
                "4. Se algum ingrediente informado não combinar com a preparação, use SUGESTAO ou INSUFICIENTE e explique isso na mensagem.");

            prompt.AppendLine(
                "5. Não invente ingredientes principais que não foram informados.");

            prompt.AppendLine(
                "6. Não adicione ingredientes extras usando expressões como 'se disponível', 'opcional', 'caso tenha' ou semelhantes.");

            prompt.AppendLine(
                "7. Se o usuário não informou alho, cebola, limão, vinagre, queijo, leite, ovos, farinha, carnes, arroz, massas, legumes ou outros alimentos, não coloque esses itens na receita.");

            prompt.AppendLine(
                "8. Os únicos itens que podem ser assumidos automaticamente são: água, sal, óleo, azeite, manteiga e temperos secos simples.");

            prompt.AppendLine(
                "9. Mesmo os itens básicos só devem ser usados quando fizerem sentido para a preparação.");

            prompt.AppendLine(
                "10. Não substitua um ingrediente informado por outro.");

            prompt.AppendLine(
                "11. Não transforme ingredientes informados em simples decoração para fingir que foram utilizados.");

            prompt.AppendLine(
                "12. A receita deve realmente aproveitar os ingredientes fornecidos.");

            prompt.AppendLine(
                "13. Respeite rigorosamente a preferência escolhida pelo usuário.");

            prompt.AppendLine(
                "14. Se a preferência não puder ser atendida com os ingredientes informados, não ignore a preferência. Use SUGESTAO ou INSUFICIENTE.");

            prompt.AppendLine(
                "15. Não faça combinações estranhas, artificiais ou pouco apetitosas apenas para usar todos os ingredientes.");

            prompt.AppendLine(
                "16. Se usar todos os ingredientes gerar uma receita ruim, não gere a receita.");

            prompt.AppendLine(
                "17. Quando não houver uma boa receita, dê uma opinião clara, educada e útil.");

            prompt.AppendLine(
                "18. Nas sugestões, informe quais ingredientes adicionais poderiam tornar a preparação coerente.");

            prompt.AppendLine(
                "19. Nunca afirme que o usuário possui um ingrediente que ele não informou.");

            prompt.AppendLine(
                "20. Use bom senso culinário como um chef profissional.");

            prompt.AppendLine();

            prompt.AppendLine(
                "DECISÃO:");

            prompt.AppendLine(
                "Use RECEITA somente quando existir uma preparação coerente, prática e recomendável com os ingredientes informados.");

            prompt.AppendLine(
                "Use SUGESTAO quando existir uma boa ideia, mas faltar algum ingrediente importante ou quando algum ingrediente informado não combinar bem.");

            prompt.AppendLine(
                "Use INSUFICIENTE quando não houver uma preparação culinária razoável.");

            prompt.AppendLine();

            prompt.AppendLine(
                "IMPORTANTE SOBRE RECEITA:");

            prompt.AppendLine(
                "Se tipoResposta for RECEITA, a lista de ingredientes deve conter os ingredientes fornecidos pelo usuário que foram utilizados.");

            prompt.AppendLine(
                "Não inclua ingredientes extras fora da lista de básicos permitidos.");

            prompt.AppendLine(
                "Não escreva 'se disponível' ou 'opcional' para alimentos que não foram informados.");

            prompt.AppendLine(
                "Os passos devem corresponder exatamente aos ingredientes listados.");

            prompt.AppendLine();

            prompt.AppendLine(
                "IMPORTANTE SOBRE SUGESTAO OU INSUFICIENTE:");

            prompt.AppendLine(
                "Não gere nome de receita, ingredientes ou passos.");

            prompt.AppendLine(
                "Explique claramente o motivo em mensagem.");

            prompt.AppendLine(
                "Forneça de 1 a 5 sugestões úteis.");

            prompt.AppendLine();

            prompt.AppendLine(
                "FORMATO:");

            prompt.AppendLine(
                "Responda SOMENTE com JSON válido.");

            prompt.AppendLine(
                "Não use Markdown.");

            prompt.AppendLine(
                "Não use bloco de código.");

            prompt.AppendLine(
                "Não escreva texto fora do JSON.");

            prompt.AppendLine();

            prompt.AppendLine(
                "Ingredientes e passos DEVEM ser listas de textos simples.");

            prompt.AppendLine(
                "Não use objetos dentro das listas.");

            prompt.AppendLine();

            prompt.AppendLine(
                "Exemplo de ingredientes:");

            prompt.AppendLine(
                "[\"500 g de carne moída\", \"4 batatas\", \"2 xícaras de arroz\"]");

            prompt.AppendLine();

            prompt.AppendLine(
                "Exemplo de passos:");

            prompt.AppendLine(
                "[\"Cozinhe as batatas.\", \"Prepare a carne.\"]");

            prompt.AppendLine();

            prompt.AppendLine(
                "JSON OBRIGATÓRIO:");

            prompt.AppendLine(
                "{");

            prompt.AppendLine(
                "  \"tipoResposta\": \"RECEITA\",");

            prompt.AppendLine(
                "  \"mensagem\": \"\",");

            prompt.AppendLine(
                "  \"sugestoes\": [],");

            prompt.AppendLine(
                "  \"nome\": \"\",");

            prompt.AppendLine(
                "  \"descricao\": \"\",");

            prompt.AppendLine(
                "  \"pais\": \"\",");

            prompt.AppendLine(
                "  \"categoria\": \"\",");

            prompt.AppendLine(
                "  \"porcoes\": " +
                porcoes +
                ",");

            prompt.AppendLine(
                "  \"tempoMinutos\": 0,");

            prompt.AppendLine(
                "  \"ingredientes\": [],");

            prompt.AppendLine(
                "  \"passos\": []");

            prompt.AppendLine(
                "}");


            return prompt.ToString();
        }


        // =========================================================
        // LIMPAR JSON
        // =========================================================

        private static string LimparJson(
            string texto)
        {
            texto =
                texto.Trim();


            if (
                texto.StartsWith(
                    "```json",
                    StringComparison.OrdinalIgnoreCase))
            {
                texto =
                    texto.Substring(7);
            }
            else if (
                texto.StartsWith(
                    "```"))
            {
                texto =
                    texto.Substring(3);
            }


            if (
                texto.EndsWith(
                    "```"))
            {
                texto =
                    texto.Substring(
                        0,
                        texto.Length - 3);
            }


            texto =
                texto.Trim();


            var inicioJson =
                texto.IndexOf('{');


            var fimJson =
                texto.LastIndexOf('}');


            if (
                inicioJson >= 0 &&
                fimJson > inicioJson)
            {
                texto =
                    texto.Substring(
                        inicioJson,
                        fimJson -
                        inicioJson +
                        1);
            }


            return texto;
        }
    }
}