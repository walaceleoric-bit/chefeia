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
            //
            // Mantemos o cliente cadastrado no Asaas
            // para relacionamento interno.
            //
            // Porém NÃO vamos enviar esse customer
            // para o Checkout agora, porque ele ainda
            // não possui CPF/endereço completos.
            //
            // O próprio Checkout solicitará esses dados.
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
                assinatura.AsaasCustomerId =
                    customerId;

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
            // DATAS DA ASSINATURA
            // =================================================

            var hoje =
                DateTime.UtcNow.Date;


            var dataFinal =
                hoje.AddYears(10);


            // =================================================
            // CHECKOUT
            //
            // NÃO enviamos customerData.
            // NÃO enviamos customer.
            //
            // Assim o próprio Asaas pede ao pagador:
            //
            // CPF/CNPJ
            // telefone
            // CEP
            // endereço
            // número
            // bairro
            //
            // diretamente na página segura do Checkout.
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


            // =================================================
            // LER RESPOSTA
            // =================================================

            using var json =
                JsonDocument.Parse(
                    conteudo
                );


            if (
                !json.RootElement.TryGetProperty(
                    "id",
                    out var idElement))
            {
                _logger.LogError(
                    "Asaas respondeu ao Checkout sem campo id. Resposta: {Resposta}",
                    conteudo
                );


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
            // LINK DO CHECKOUT
            // =================================================

            var checkoutUrl =
                $"https://asaas.com/checkoutSession/show?id={Uri.EscapeDataString(checkoutId)}";


            _logger.LogInformation(
                "Checkout Premium criado com sucesso. Usuário {UserId}. Checkout {CheckoutId}.",
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


            request.Headers
                .TryAddWithoutValidation(
                    "Accept",
                    "application/json"
                );
        }
    }
}