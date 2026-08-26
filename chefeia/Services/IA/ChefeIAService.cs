using chefeia.Models;
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

        public ChefeIAService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ChefeIAService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ReceitaIA> SugerirReceitaAsync(
            ConsultaReceitaIA consulta)
        {
            // =====================================================
            // 1. CONFIGURAÇÕES DA RAPIDAPI
            // =====================================================

            var apiKey = _configuration["RapidApi:Key"];
            var apiHost = _configuration["RapidApi:Host"];
            var apiUrl = _configuration["RapidApi:Url"];

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
            // 2. DADOS DO USUÁRIO
            // =====================================================

            var ingredientes = string.Join(
                ", ",
                consulta.Ingredientes
            );

            var preferencia =
                string.IsNullOrWhiteSpace(consulta.Preferencia)
                    ? "qualquer tipo de receita"
                    : consulta.Preferencia;

            var porcoes =
                consulta.Porcoes > 0
                    ? consulta.Porcoes
                    : 1;

            // =====================================================
            // 3. PROMPT
            // =====================================================

            var prompt = MontarPrompt(
                ingredientes,
                preferencia,
                porcoes
            );

            // =====================================================
            // 4. CORPO DA REQUISIÇÃO
            // =====================================================

            var corpoRequisicao = new
            {
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },

                web_access = false
            };

            var jsonRequisicao =
                JsonSerializer.Serialize(corpoRequisicao);

            // =====================================================
            // 5. REQUISIÇÃO HTTP
            // =====================================================

            using var request = new HttpRequestMessage(
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

            request.Content = new StringContent(
                jsonRequisicao,
                Encoding.UTF8,
                "application/json"
            );

            // =====================================================
            // 6. LOG ANTES DA CONSULTA
            // =====================================================

            _logger.LogInformation(
                "========== CHEFE IA =========="
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

            var cronometro =
                Stopwatch.StartNew();

            // =====================================================
            // 7. ENVIAR PARA RAPIDAPI
            // =====================================================

            HttpResponseMessage response;

            try
            {
                response =
                    await _httpClient.SendAsync(request);
            }
            catch (Exception ex)
            {
                cronometro.Stop();

                _logger.LogError(
                    ex,
                    "Falha de comunicação com a RapidAPI."
                );

                throw;
            }

            using (response)
            {
                cronometro.Stop();

                var conteudoResposta =
                    await response.Content.ReadAsStringAsync();

                // =================================================
                // 8. MONITORAMENTO
                // =================================================

                _logger.LogInformation(
                    "Status HTTP: {StatusCode}",
                    (int)response.StatusCode
                );

                _logger.LogInformation(
                    "Tempo da consulta: {Tempo} ms",
                    cronometro.ElapsedMilliseconds
                );

                MostrarLimitesRapidApi(response);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "Resultado: SUCESSO"
                    );
                }
                else
                {
                    _logger.LogWarning(
                        "Resultado: ERRO"
                    );
                }

                _logger.LogInformation(
                    "=============================="
                );

                // =================================================
                // 9. VERIFICAR ERRO HTTP
                // =================================================

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        "Erro ao consultar a IA. " +
                        "Status HTTP: " +
                        (int)response.StatusCode +
                        ". Resposta: " +
                        conteudoResposta
                    );
                }

                // =================================================
                // 10. LER CAMPO result
                // =================================================

                using var documento =
                    JsonDocument.Parse(conteudoResposta);

                var raiz =
                    documento.RootElement;

                if (!raiz.TryGetProperty(
                    "result",
                    out var resultElement))
                {
                    throw new InvalidOperationException(
                        "A RapidAPI respondeu, mas não retornou o campo 'result'."
                    );
                }

                var resultadoIA =
                    resultElement.GetString();

                if (string.IsNullOrWhiteSpace(resultadoIA))
                {
                    throw new InvalidOperationException(
                        "A IA retornou uma resposta vazia."
                    );
                }

                // =================================================
                // 11. LIMPAR JSON
                // =================================================

                resultadoIA =
                    LimparJson(resultadoIA);

                // =================================================
                // 12. CONVERTER PARA ReceitaIA
                // =================================================

                var opcoesJson =
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
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
                // 13. GARANTIR VALORES
                // =================================================

                if (receita.Porcoes <= 0)
                {
                    receita.Porcoes = porcoes;
                }

                receita.Ingredientes ??=
                    new List<string>();

                receita.Passos ??=
                    new List<string>();

                return receita;
            }
        }

        // =========================================================
        // MOSTRAR LIMITES DA RAPIDAPI
        // =========================================================

        private void MostrarLimitesRapidApi(
            HttpResponseMessage response)
        {
            var encontrouLimite = false;

            foreach (var header in response.Headers)
            {
                var nome =
                    header.Key.ToLowerInvariant();

                if (
                    nome.Contains("ratelimit") ||
                    nome.Contains("rate-limit") ||
                    nome.Contains("quota") ||
                    nome.Contains("remaining"))
                {
                    encontrouLimite = true;

                    _logger.LogInformation(
                        "RapidAPI {Header}: {Valor}",
                        header.Key,
                        string.Join(", ", header.Value)
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
        // MONTAR PROMPT
        // =========================================================

        private static string MontarPrompt(
            string ingredientes,
            string preferencia,
            int porcoes)
        {
            var prompt =
                new StringBuilder();

            prompt.AppendLine(
                "Você é o Chefe IA, um especialista em culinária."
            );

            prompt.AppendLine();

            prompt.AppendLine(
                "Crie UMA receita usando principalmente os ingredientes informados."
            );

            prompt.AppendLine();

            prompt.AppendLine(
                "Ingredientes disponíveis:"
            );

            prompt.AppendLine(
                ingredientes
            );

            prompt.AppendLine();

            prompt.AppendLine(
                "Preferência: " + preferencia
            );

            prompt.AppendLine(
                "Número de porções: " + porcoes
            );

            prompt.AppendLine();

            prompt.AppendLine(
                "Você pode acrescentar ingredientes básicos necessários, " +
                "como sal, açúcar, água, óleo, manteiga e temperos."
            );

            prompt.AppendLine();

            prompt.AppendLine(
                "A receita deve ser prática, coerente e segura."
            );

            prompt.AppendLine();

            prompt.AppendLine(
                "Responda SOMENTE com JSON válido."
            );

            prompt.AppendLine(
                "Não use Markdown."
            );

            prompt.AppendLine(
                "Não use blocos de código."
            );

            prompt.AppendLine(
                "Não escreva nenhuma explicação antes ou depois do JSON."
            );

            prompt.AppendLine();

            prompt.AppendLine(
                "O JSON deve possuir exatamente estes campos:"
            );

            prompt.AppendLine(
                "nome, descricao, pais, categoria, porcoes, " +
                "tempoMinutos, ingredientes e passos."
            );

            prompt.AppendLine();

            prompt.AppendLine(
                "Exemplo do formato esperado:"
            );

            prompt.AppendLine("{");

            prompt.AppendLine(
                "  \"nome\": \"Nome da receita\","
            );

            prompt.AppendLine(
                "  \"descricao\": \"Descrição curta\","
            );

            prompt.AppendLine(
                "  \"pais\": \"Brasil\","
            );

            prompt.AppendLine(
                "  \"categoria\": \"Prato principal\","
            );

            prompt.AppendLine(
                "  \"porcoes\": " + porcoes + ","
            );

            prompt.AppendLine(
                "  \"tempoMinutos\": 30,"
            );

            prompt.AppendLine(
                "  \"ingredientes\": ["
            );

            prompt.AppendLine(
                "    \"500 g de ingrediente\","
            );

            prompt.AppendLine(
                "    \"1 unidade de outro ingrediente\""
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

            prompt.AppendLine("}");

            return prompt.ToString();
        }

        // =========================================================
        // LIMPAR JSON RETORNADO PELA IA
        // =========================================================

        private static string LimparJson(
            string texto)
        {
            texto =
                texto.Trim();

            if (texto.StartsWith(
                "```json",
                StringComparison.OrdinalIgnoreCase))
            {
                texto =
                    texto.Substring(7);
            }
            else if (texto.StartsWith("```"))
            {
                texto =
                    texto.Substring(3);
            }

            if (texto.EndsWith("```"))
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
                        fimJson - inicioJson + 1
                    );
            }

            return texto;
        }
    }
}