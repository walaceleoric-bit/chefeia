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
            // 1. CONFIGURAÇÕES DA RAPIDAPI
            // =====================================================

            var apiKey =
                _configuration["RapidApi:Key"];

            var apiHost =
                _configuration["RapidApi:Host"];

            var apiUrl =
                _configuration["RapidApi:Url"];


            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "A chave RapidApi:Key não foi configurada."
                );
            }


            if (string.IsNullOrWhiteSpace(apiHost))
            {
                throw new InvalidOperationException(
                    "RapidApi:Host não foi configurado."
                );
            }


            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                throw new InvalidOperationException(
                    "RapidApi:Url não foi configurado."
                );
            }


            // =====================================================
            // 2. VALIDAR CONSULTA
            // =====================================================

            if (
                consulta.Ingredientes == null ||
                consulta.Ingredientes.Count == 0)
            {
                throw new ArgumentException(
                    "Informe pelo menos um ingrediente."
                );
            }


            var ingredientes =
                string.Join(
                    ", ",
                    consulta.Ingredientes
                );


            var preferencia =
                string.IsNullOrWhiteSpace(
                    consulta.Preferencia)
                    ? "qualquer tipo de receita"
                    : consulta.Preferencia;


            var porcoes =
                consulta.Porcoes > 0
                    ? consulta.Porcoes
                    : 1;


            // =====================================================
            // 3. IDENTIFICAR USUÁRIO
            // =====================================================

            var httpContext =
                _httpContextAccessor.HttpContext;


            if (httpContext == null)
            {
                throw new UnauthorizedAccessException(
                    "Não foi possível identificar o usuário."
                );
            }


            var usuario =
                await _userManager.GetUserAsync(
                    httpContext.User
                );


            if (usuario == null)
            {
                throw new UnauthorizedAccessException(
                    "Faça login para utilizar o Chefe IA."
                );
            }


            if (!usuario.IsActive)
            {
                throw new UnauthorizedAccessException(
                    "Esta conta está desativada."
                );
            }


            // =====================================================
            // 4. PLANO DO USUÁRIO
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
                planCode = "FREE";
            }


            // =====================================================
            // 5. REGISTRO DE CONSUMO
            // =====================================================

            var consumo =
                new AiUsage
                {
                    CreatedAt =
                        DateTime.UtcNow,

                    Success =
                        false,

                    IngredientCount =
                        consulta.Ingredientes.Count,

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
            // 6. PROMPT
            // =====================================================

            var prompt =
                MontarPrompt(
                    ingredientes,
                    preferencia,
                    porcoes
                );


            // =====================================================
            // 7. CORPO DA RAPIDAPI
            // =====================================================

            var corpoRequisicao =
                new
                {
                    messages =
                        new[]
                        {
                            new
                            {
                                role = "user",
                                content = prompt
                            }
                        },

                    system_prompt =
                        "Você é o Chefe IA, um assistente especializado em culinária. " +
                        "Responda sempre em português do Brasil e siga exatamente " +
                        "o formato solicitado pelo usuário.",

                    temperature = 0.7,

                    top_k = 5,

                    top_p = 0.9,

                    max_tokens = 1200,

                    web_access = false
                };


            var jsonRequisicao =
                JsonSerializer.Serialize(
                    corpoRequisicao
                );


            // =====================================================
            // 8. CRIAR REQUISIÇÃO
            // =====================================================

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    apiUrl
                );


            request.Headers.Add(
                "x-rapidapi-key",
                apiKey
            );


            request.Headers.Add(
                "x-rapidapi-host",
                apiHost
            );


            request.Content =
                new StringContent(
                    jsonRequisicao,
                    Encoding.UTF8,
                    "application/json"
                );


            // =====================================================
            // 9. LOG
            // =====================================================

            _logger.LogInformation(
                "========== CHEFE IA =========="
            );


            _logger.LogInformation(
                "Usuário: {UsuarioId}",
                usuario.Id
            );


            _logger.LogInformation(
                "Plano: {Plano}",
                planCode
            );


            _logger.LogInformation(
                "Enviando consulta para a RapidAPI."
            );


            _logger.LogInformation(
                "Ingredientes informados: {Quantidade}",
                consulta.Ingredientes.Count
            );


            _logger.LogInformation(
                "Porções: {Porcoes}",
                porcoes
            );


            _logger.LogInformation(
                "Preferência: {Preferencia}",
                preferencia
            );


            // =====================================================
            // 10. CRONÔMETRO
            // =====================================================

            var cronometro =
                Stopwatch.StartNew();


            HttpResponseMessage? response = null;


            try
            {
                // =================================================
                // 11. CHAMAR RAPIDAPI
                // =================================================

                response =
                    await _httpClient.SendAsync(
                        request
                    );


                cronometro.Stop();


                // =================================================
                // 12. LER RESPOSTA
                // =================================================

                var conteudoResposta =
                    await response.Content
                        .ReadAsStringAsync();


                consumo.StatusCode =
                    (int)response.StatusCode;


                consumo.DurationMs =
                    cronometro.ElapsedMilliseconds;


                // =================================================
                // 13. LIMITES DA RAPIDAPI
                // =================================================

                consumo.RequestsLimit =
                    ObterHeaderInt(
                        response,
                        "X-RateLimit-Requests-Limit"
                    );


                consumo.RequestsRemaining =
                    ObterHeaderInt(
                        response,
                        "X-RateLimit-Requests-Remaining"
                    );


                consumo.CreditLimit =
                    ObterHeaderInt(
                        response,
                        "X-RateLimit-Credit-Limit"
                    );


                consumo.CreditRemaining =
                    ObterHeaderInt(
                        response,
                        "X-RateLimit-Credit-Remaining"
                    );


                // =================================================
                // 14. LOG DA RESPOSTA
                // =================================================

                _logger.LogInformation(
                    "Status HTTP: {StatusCode}",
                    consumo.StatusCode
                );


                _logger.LogInformation(
                    "Tempo da consulta: {Tempo} ms",
                    consumo.DurationMs
                );


                MostrarLimitesRapidApi(
                    response
                );


                // =================================================
                // 15. ERRO HTTP
                // =================================================

                if (!response.IsSuccessStatusCode)
                {
                    consumo.Success =
                        false;


                    consumo.ErrorMessage =
                        LimitarTexto(
                            "Erro HTTP " +
                            (int)response.StatusCode +
                            ". " +
                            conteudoResposta,
                            2000
                        );


                    await SalvarConsumoAsync(
                        consumo
                    );


                    _logger.LogWarning(
                        "Resultado: ERRO"
                    );


                    _logger.LogInformation(
                        "=============================="
                    );


                    throw new HttpRequestException(
                        "Erro ao consultar a IA. " +
                        "Status HTTP: " +
                        (int)response.StatusCode +
                        ". Resposta: " +
                        conteudoResposta
                    );
                }


                // =================================================
                // 16. INTERPRETAR RESPOSTA
                // =================================================

                using var documento =
                    JsonDocument.Parse(
                        conteudoResposta
                    );


                var raiz =
                    documento.RootElement;


                if (
                    raiz.TryGetProperty(
                        "status",
                        out var statusElement))
                {
                    if (
                        statusElement.ValueKind ==
                        JsonValueKind.False)
                    {
                        throw new InvalidOperationException(
                            "A RapidAPI informou que a consulta não foi concluída."
                        );
                    }
                }


                if (
                    !raiz.TryGetProperty(
                        "result",
                        out var resultElement))
                {
                    throw new InvalidOperationException(
                        "A RapidAPI respondeu, mas não retornou o campo 'result'."
                    );
                }


                var resultadoIA =
                    resultElement.GetString();


                if (
                    string.IsNullOrWhiteSpace(
                        resultadoIA))
                {
                    throw new InvalidOperationException(
                        "A IA retornou uma resposta vazia."
                    );
                }


                // =================================================
                // 17. LIMPAR JSON
                // =================================================

                resultadoIA =
                    LimparJson(
                        resultadoIA
                    );


                // =================================================
                // 18. CONVERTER PARA RECEITA
                // =================================================

                var opcoesJson =
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive =
                            true
                    };


                ReceitaIA? receita;


                try
                {
                    receita =
                        JsonSerializer.Deserialize<ReceitaIA>(
                            resultadoIA,
                            opcoesJson
                        );
                }
                catch (JsonException ex)
                {
                    throw new InvalidOperationException(
                        "A IA respondeu, mas a receita não veio em JSON válido.",
                        ex
                    );
                }


                if (receita == null)
                {
                    throw new InvalidOperationException(
                        "Não foi possível transformar a resposta da IA em uma receita."
                    );
                }


                // =================================================
                // 19. GARANTIR VALORES
                // =================================================

                if (receita.Porcoes <= 0)
                {
                    receita.Porcoes =
                        porcoes;
                }


                receita.Ingredientes ??=
                    new List<string>();


                receita.Passos ??=
                    new List<string>();


                // =================================================
                // 20. REGISTRAR CONSUMO COM SUCESSO
                // =================================================

                consumo.Success =
                    true;


                consumo.ErrorMessage =
                    null;


                await SalvarConsumoAsync(
                    consumo
                );


                // =================================================
                // 21. HISTÓRICO PREMIUM
                // =================================================

                if (planCode == "PREMIUM")
                {
                    await SalvarHistoricoAsync(
                        usuario,
                        receita,
                        consulta,
                        preferencia
                    );
                }


                // =================================================
                // 22. LOG FINAL
                // =================================================

                _logger.LogInformation(
                    "Resultado: SUCESSO"
                );


                _logger.LogInformation(
                    "Consumo registrado no PostgreSQL."
                );


                if (planCode == "PREMIUM")
                {
                    _logger.LogInformation(
                        "Receita registrada no histórico Premium."
                    );
                }


                _logger.LogInformation(
                    "Usuário: {UsuarioId} | Plano: {Plano}",
                    usuario.Id,
                    planCode
                );


                _logger.LogInformation(
                    "=============================="
                );


                return receita;
            }
            catch (Exception ex)
            {
                if (cronometro.IsRunning)
                {
                    cronometro.Stop();
                }


                // =================================================
                // SALVAR ERRO SE AINDA NÃO FOI SALVO
                // =================================================

                if (consumo.Id == 0)
                {
                    consumo.Success =
                        false;


                    consumo.DurationMs =
                        cronometro.ElapsedMilliseconds;


                    consumo.ErrorMessage =
                        LimitarTexto(
                            ex.Message,
                            2000
                        );


                    if (response != null)
                    {
                        consumo.StatusCode =
                            (int)response.StatusCode;


                        consumo.RequestsLimit ??=
                            ObterHeaderInt(
                                response,
                                "X-RateLimit-Requests-Limit"
                            );


                        consumo.RequestsRemaining ??=
                            ObterHeaderInt(
                                response,
                                "X-RateLimit-Requests-Remaining"
                            );


                        consumo.CreditLimit ??=
                            ObterHeaderInt(
                                response,
                                "X-RateLimit-Credit-Limit"
                            );


                        consumo.CreditRemaining ??=
                            ObterHeaderInt(
                                response,
                                "X-RateLimit-Credit-Remaining"
                            );
                    }


                    await SalvarConsumoAsync(
                        consumo
                    );
                }


                _logger.LogError(
                    ex,
                    "Falha na consulta do Chefe IA."
                );


                _logger.LogInformation(
                    "=============================="
                );


                throw;
            }
            finally
            {
                response?.Dispose();
            }
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
                    consumo
                );


                await _dbContext
                    .SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Não foi possível registrar o consumo da IA no banco."
                );
            }
        }


        // =========================================================
        // SALVAR HISTÓRICO PREMIUM
        // =========================================================

        private async Task SalvarHistoricoAsync(
            AppUser usuario,
            ReceitaIA receita,
            ConsultaReceitaIA consulta,
            string preferencia)
        {
            try
            {
                var ingredientesJson =
                    JsonSerializer.Serialize(
                        receita.Ingredientes ??
                        new List<string>()
                    );


                var passosJson =
                    JsonSerializer.Serialize(
                        receita.Passos ??
                        new List<string>()
                    );


                var ingredientesSolicitados =
                    string.Join(
                        ", ",
                        consulta.Ingredientes ??
                        new List<string>()
                    );


                var historico =
                    new RecipeHistory
                    {
                        UserId =
                            usuario.Id,

                        Name =
                            receita.Nome ??
                            "Receita sem nome",

                        Country =
                            receita.Pais ??
                            string.Empty,

                        Category =
                            receita.Categoria ??
                            string.Empty,

                        Description =
                            receita.Descricao ??
                            string.Empty,

                        PreparationMinutes =
                            receita.TempoMinutos,

                        Servings =
                            receita.Porcoes,

                        IngredientsJson =
                            ingredientesJson,

                        StepsJson =
                            passosJson,

                        RequestedIngredients =
                            LimitarTexto(
                                ingredientesSolicitados,
                                1000
                            ),

                        Preference =
                            LimitarTexto(
                                preferencia,
                                150
                            ),

                        CreatedAt =
                            DateTime.UtcNow
                    };


                _dbContext.RecipeHistories.Add(
                    historico
                );


                await _dbContext
                    .SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Um erro no histórico não deve impedir
                // o usuário de receber a receita.

                _logger.LogError(
                    ex,
                    "Não foi possível salvar a receita no histórico Premium."
                );
            }
        }


        // =========================================================
        // PEGAR HEADER NUMÉRICO
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


            var valor =
                valores.FirstOrDefault();


            if (
                int.TryParse(
                    valor,
                    out var numero))
            {
                return numero;
            }


            return null;
        }


        // =========================================================
        // MOSTRAR LIMITES DA RAPIDAPI
        // =========================================================

        private void MostrarLimitesRapidApi(
            HttpResponseMessage response)
        {
            var encontrouLimite =
                false;


            foreach (
                var header
                in response.Headers)
            {
                var nome =
                    header.Key
                        .ToLowerInvariant();


                if (
                    nome.Contains("ratelimit") ||
                    nome.Contains("rate-limit") ||
                    nome.Contains("quota") ||
                    nome.Contains("remaining") ||
                    nome.Contains("credit"))
                {
                    encontrouLimite =
                        true;


                    _logger.LogInformation(
                        "RapidAPI {Header}: {Valor}",
                        header.Key,
                        string.Join(
                            ", ",
                            header.Value
                        )
                    );
                }
            }


            if (!encontrouLimite)
            {
                _logger.LogInformation(
                    "A API não informou limite/restante nos headers desta resposta."
                );
            }
        }


        // =========================================================
        // LIMITAR TEXTO
        // =========================================================

        private static string LimitarTexto(
            string texto,
            int tamanhoMaximo)
        {
            if (string.IsNullOrEmpty(
                texto))
            {
                return string.Empty;
            }


            if (
                texto.Length <=
                tamanhoMaximo)
            {
                return texto;
            }


            return texto.Substring(
                0,
                tamanhoMaximo
            );
        }


        // =========================================================
        // PROMPT DO CHEFE IA
        // =========================================================

        private static string MontarPrompt(
            string ingredientes,
            string preferencia,
            int porcoes)
        {
            var prompt =
                new StringBuilder();


            prompt.AppendLine(
                "Crie UMA receita culinária usando principalmente os ingredientes abaixo."
            );


            prompt.AppendLine();


            prompt.AppendLine(
                "INGREDIENTES DISPONÍVEIS:"
            );


            prompt.AppendLine(
                ingredientes
            );


            prompt.AppendLine();


            prompt.AppendLine(
                "PREFERÊNCIA: " +
                preferencia
            );


            prompt.AppendLine(
                "PORÇÕES: " +
                porcoes
            );


            prompt.AppendLine();


            prompt.AppendLine(
                "Você pode acrescentar ingredientes básicos necessários, " +
                "como sal, açúcar, água, óleo, azeite, manteiga e temperos."
            );


            prompt.AppendLine();


            prompt.AppendLine(
                "Não invente processos culinários perigosos ou incoerentes."
            );


            prompt.AppendLine(
                "A receita deve ser prática, clara e possível de preparar."
            );


            prompt.AppendLine();


            prompt.AppendLine(
                "Responda SOMENTE com JSON válido."
            );


            prompt.AppendLine(
                "Não use Markdown."
            );


            prompt.AppendLine(
                "Não escreva ```json."
            );


            prompt.AppendLine(
                "Não escreva nenhuma explicação antes ou depois do JSON."
            );


            prompt.AppendLine();


            prompt.AppendLine(
                "Use exatamente estes campos:"
            );


            prompt.AppendLine(
                "nome, descricao, pais, categoria, porcoes, " +
                "tempoMinutos, ingredientes e passos."
            );


            prompt.AppendLine();


            prompt.AppendLine(
                "Formato esperado:"
            );


            prompt.AppendLine(
                "{"
            );


            prompt.AppendLine(
                "  \"nome\": \"Nome da receita\","
            );


            prompt.AppendLine(
                "  \"descricao\": \"Descrição curta da receita\","
            );


            prompt.AppendLine(
                "  \"pais\": \"País ou origem culinária\","
            );


            prompt.AppendLine(
                "  \"categoria\": \"Categoria da receita\","
            );


            prompt.AppendLine(
                "  \"porcoes\": " +
                porcoes +
                ","
            );


            prompt.AppendLine(
                "  \"tempoMinutos\": 30,"
            );


            prompt.AppendLine(
                "  \"ingredientes\": ["
            );


            prompt.AppendLine(
                "    \"Ingrediente com quantidade\","
            );


            prompt.AppendLine(
                "    \"Outro ingrediente com quantidade\""
            );


            prompt.AppendLine(
                "  ],"
            );


            prompt.AppendLine(
                "  \"passos\": ["
            );


            prompt.AppendLine(
                "    \"Primeiro passo\","
            );


            prompt.AppendLine(
                "    \"Segundo passo\""
            );


            prompt.AppendLine(
                "  ]"
            );


            prompt.AppendLine(
                "}"
            );


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
                        texto.Length - 3
                    );
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
                        1
                    );
            }


            return texto;
        }
    }
}