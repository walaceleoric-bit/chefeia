using chefeia.Data;
using chefeia.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace chefeia.Services.Asaas
{
    public class AsaasService : IAsaasService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _dbContext;
        private readonly AsaasOptions _options;
        private readonly ILogger<AsaasService> _logger;


        public AsaasService(
            HttpClient httpClient,
            AppDbContext dbContext,
            IOptions<AsaasOptions> options,
            ILogger<AsaasService> logger)
        {
            _httpClient = httpClient;
            _dbContext = dbContext;
            _options = options.Value;
            _logger = logger;
        }


        // =====================================================
        // CRIAR OU OBTER CLIENTE ASAAS
        // =====================================================

        public async Task<string> CriarOuObterClienteAsync(
            AppUser usuario,
            CancellationToken cancellationToken = default)
        {
            ValidarConfiguracao();


            var assinatura =
                await _dbContext
                    .UserSubscriptions
                    .FirstOrDefaultAsync(
                        x =>
                            x.UserId ==
                            usuario.Id,
                        cancellationToken
                    );


            if (
                assinatura != null &&
                !string.IsNullOrWhiteSpace(
                    assinatura.AsaasCustomerId))
            {
                return assinatura
                    .AsaasCustomerId;
            }


            if (string.IsNullOrWhiteSpace(
                usuario.Email))
            {
                throw new InvalidOperationException(
                    "O usuário não possui e-mail cadastrado."
                );
            }


            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    $"{_options.BaseUrl.TrimEnd('/')}/customers"
                );


            AdicionarHeaders(
                request
            );


            var body =
                new
                {
                    name =
                        string.IsNullOrWhiteSpace(
                            usuario.Name)
                            ? usuario.Email
                            : usuario.Name,

                    email =
                        usuario.Email,

                    externalReference =
                        usuario.Id
                };


            request.Content =
                JsonContent.Create(
                    body
                );


            using var response =
                await _httpClient.SendAsync(
                    request,
                    cancellationToken
                );


            var conteudo =
                await response.Content
                    .ReadAsStringAsync(
                        cancellationToken
                    );


            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Erro ao criar cliente Asaas. Status {Status}. Resposta: {Resposta}",
                    (int)response.StatusCode,
                    conteudo
                );


                throw new InvalidOperationException(
                    "Não foi possível criar o cliente no Asaas."
                );
            }


            using var json =
                JsonDocument.Parse(
                    conteudo
                );


            if (
                !json.RootElement.TryGetProperty(
                    "id",
                    out var idElement))
            {
                throw new InvalidOperationException(
                    "O Asaas não retornou o ID do cliente."
                );
            }


            var customerId =
                idElement.GetString();


            if (string.IsNullOrWhiteSpace(
                customerId))
            {
                throw new InvalidOperationException(
                    "O ID do cliente retornado pelo Asaas está vazio."
                );
            }


            if (assinatura == null)
            {
                assinatura =
                    new UserSubscription
                    {
                        UserId =
                            usuario.Id,

                        AsaasCustomerId =
                            customerId,

                        PlanCode =
                            "PREMIUM",

                        Price =
                            0,

                        Status =
                            "PENDING",

                        IsActive =
                            false,

                        CreatedAt =
                            DateTime.UtcNow
                    };


                _dbContext
                    .UserSubscriptions
                    .Add(
                        assinatura
                    );
            }
            else
            {
                assinatura
                    .AsaasCustomerId =
                        customerId;
            }


            await _dbContext
                .SaveChangesAsync(
                    cancellationToken
                );


            return customerId;
        }


        // =====================================================
        // CRIAR CHECKOUT PREMIUM
        // =====================================================

        public async Task<string> CriarCheckoutPremiumAsync(
            AppUser usuario,
            decimal preco,
            string successUrl,
            string cancelUrl,
            string expiredUrl,
            CancellationToken cancellationToken = default)
        {
            ValidarConfiguracao();


            if (preco <= 0)
            {
                throw new InvalidOperationException(
                    "O preço do Premium precisa ser maior que zero."
                );
            }


            if (string.IsNullOrWhiteSpace(
                usuario.Email))
            {
                throw new InvalidOperationException(
                    "O usuário precisa possuir um e-mail cadastrado."
                );
            }


            // =================================================
            // GARANTIR CLIENTE NO ASAAS
            // =================================================

            var customerId =
                await CriarOuObterClienteAsync(
                    usuario,
                    cancellationToken
                );


            // =================================================
            // REGISTRO LOCAL DA ASSINATURA
            // =================================================

            var assinatura =
                await _dbContext
                    .UserSubscriptions
                    .FirstOrDefaultAsync(
                        x =>
                            x.UserId ==
                            usuario.Id,
                        cancellationToken
                    );


            if (assinatura == null)
            {
                assinatura =
                    new UserSubscription
                    {
                        UserId =
                            usuario.Id,

                        AsaasCustomerId =
                            customerId,

                        PlanCode =
                            "PREMIUM",

                        Price =
                            preco,

                        Status =
                            "PENDING",

                        BillingType =
                            "CREDIT_CARD",

                        IsActive =
                            false,

                        CreatedAt =
                            DateTime.UtcNow
                    };


                _dbContext
                    .UserSubscriptions
                    .Add(
                        assinatura
                    );
            }
            else
            {
                assinatura.Price =
                    preco;

                assinatura.PlanCode =
                    "PREMIUM";

                assinatura.Status =
                    "PENDING";

                assinatura.BillingType =
                    "CREDIT_CARD";

                assinatura.IsActive =
                    false;

                assinatura.CanceledAt =
                    null;
            }


            await _dbContext
                .SaveChangesAsync(
                    cancellationToken
                );


            // =================================================
            // PRIMEIRO VENCIMENTO
            //
            // Hoje: pagamento inicial.
            // =================================================

            var hoje =
                DateTime.UtcNow.Date;


            // Mantemos uma duração longa para a recorrência.
            // Depois poderemos trocar por cancelamento explícito.
            var dataFinal =
                hoje.AddYears(10);


            // =================================================
            // BODY DO CHECKOUT
            // =================================================

            var body =
                new
                {
                    billingTypes =
                        new[]
                        {
                            "CREDIT_CARD"
                        },

                    chargeTypes =
                        new[]
                        {
                            "RECURRENT"
                        },

                    minutesToExpire =
                        60,

                    externalReference =
                        $"CHEFEIA-PREMIUM-{usuario.Id}",

                    callback =
                        new
                        {
                            successUrl =
                                successUrl,

                            cancelUrl =
                                cancelUrl,

                            expiredUrl =
                                expiredUrl
                        },

                    items =
                        new[]
                        {
                            new
                            {
                                name =
                                    "Chefe IA Premium",

                                description =
                                    "Assinatura mensal do Chefe IA Premium",

                                quantity =
                                    1,

                                value =
                                    preco
                            }
                        },

                    customerData =
                        new
                        {
                            name =
                                string.IsNullOrWhiteSpace(
                                    usuario.Name)
                                    ? usuario.Email
                                    : usuario.Name,

                            email =
                                usuario.Email
                        },

                    subscription =
                        new
                        {
                            cycle =
                                "MONTHLY",

                            nextDueDate =
                                hoje.ToString(
                                    "yyyy-MM-dd"
                                ),

                            endDate =
                                dataFinal.ToString(
                                    "yyyy-MM-dd"
                                )
                        }
                };


            // =================================================
            // REQUEST
            // =================================================

            using var request =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    $"{_options.BaseUrl.TrimEnd('/')}/checkouts"
                );


            AdicionarHeaders(
                request
            );


            request.Content =
                JsonContent.Create(
                    body
                );


            using var response =
                await _httpClient.SendAsync(
                    request,
                    cancellationToken
                );


            var conteudo =
                await response.Content
                    .ReadAsStringAsync(
                        cancellationToken
                    );


            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Erro ao criar Checkout Asaas. Status {Status}. Resposta: {Resposta}",
                    (int)response.StatusCode,
                    conteudo
                );


                throw new InvalidOperationException(
                    "Não foi possível criar o Checkout do Premium."
                );
            }


            using var json =
                JsonDocument.Parse(
                    conteudo
                );


            if (
                !json.RootElement.TryGetProperty(
                    "id",
                    out var idElement))
            {
                throw new InvalidOperationException(
                    "O Asaas não retornou o ID do Checkout."
                );
            }


            var checkoutId =
                idElement.GetString();


            if (string.IsNullOrWhiteSpace(
                checkoutId))
            {
                throw new InvalidOperationException(
                    "O ID do Checkout retornado pelo Asaas está vazio."
                );
            }


            // =================================================
            // MONTAR LINK
            // =================================================

            var checkoutUrl =
                $"https://asaas.com/checkoutSession/show?id={Uri.EscapeDataString(checkoutId)}";


            _logger.LogInformation(
                "Checkout Premium criado no Asaas. Usuário {UserId}. Checkout {CheckoutId}.",
                usuario.Id,
                checkoutId
            );


            return checkoutUrl;
        }


        // =====================================================
        // VALIDAR CONFIGURAÇÃO
        // =====================================================

        private void ValidarConfiguracao()
        {
            if (string.IsNullOrWhiteSpace(
                _options.BaseUrl))
            {
                throw new InvalidOperationException(
                    "Asaas:BaseUrl não foi configurada."
                );
            }


            if (string.IsNullOrWhiteSpace(
                _options.ApiKey))
            {
                throw new InvalidOperationException(
                    "Asaas:ApiKey não foi configurada."
                );
            }
        }


        // =====================================================
        // HEADERS ASAAS
        // =====================================================

        private void AdicionarHeaders(
            HttpRequestMessage request)
        {
            request.Headers
                .TryAddWithoutValidation(
                    "access_token",
                    _options.ApiKey
                );


            request.Headers
                .TryAddWithoutValidation(
                    "User-Agent",
                    "ChefeIA/1.0"
                );
        }
    }
}