using chefeia.Data;
using chefeia.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace chefeia.Controllers.Api
{
    [ApiController]
    [Route("api/webhooks/asaas")]
    public class AsaasWebhookController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly AsaasOptions _options;
        private readonly ILogger<AsaasWebhookController> _logger;


        public AsaasWebhookController(
            AppDbContext dbContext,
            UserManager<AppUser> userManager,
            IOptions<AsaasOptions> options,
            ILogger<AsaasWebhookController> logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _options = options.Value;
            _logger = logger;
        }


        // =====================================================
        // WEBHOOK ASAAS
        // =====================================================

        [HttpPost]
        public async Task<IActionResult> Receber(
            CancellationToken cancellationToken)
        {
            // =================================================
            // VALIDAR TOKEN
            // =================================================

            if (string.IsNullOrWhiteSpace(
                _options.WebhookToken))
            {
                _logger.LogError(
                    "Asaas:WebhookToken não está configurado."
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError
                );
            }


            var tokenRecebido =
                Request.Headers[
                    "asaas-access-token"
                ]
                .FirstOrDefault();


            if (
                string.IsNullOrWhiteSpace(
                    tokenRecebido) ||
                !string.Equals(
                    tokenRecebido,
                    _options.WebhookToken,
                    StringComparison.Ordinal
                ))
            {
                _logger.LogWarning(
                    "Tentativa de acesso ao webhook Asaas com token inválido."
                );

                return Unauthorized();
            }


            // =================================================
            // LER JSON
            // =================================================

            string payload;


            using (
                var reader =
                    new StreamReader(
                        Request.Body
                    ))
            {
                payload =
                    await reader
                        .ReadToEndAsync(
                            cancellationToken
                        );
            }


            if (string.IsNullOrWhiteSpace(
                payload))
            {
                return BadRequest();
            }


            // =================================================
            // INTERPRETAR JSON
            // =================================================

            JsonDocument documento;


            try
            {
                documento =
                    JsonDocument.Parse(
                        payload
                    );
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "Webhook Asaas recebeu JSON inválido."
                );

                return BadRequest();
            }


            using (documento)
            {
                var raiz =
                    documento.RootElement;


                // =============================================
                // ID DO EVENTO
                // =============================================

                var eventId =
                    ObterString(
                        raiz,
                        "id"
                    );


                var eventType =
                    ObterString(
                        raiz,
                        "event"
                    );


                if (
                    string.IsNullOrWhiteSpace(
                        eventId) ||
                    string.IsNullOrWhiteSpace(
                        eventType))
                {
                    _logger.LogWarning(
                        "Webhook Asaas recebido sem id ou event."
                    );

                    return BadRequest();
                }


                // =============================================
                // IDEMPOTÊNCIA
                //
                // Se esse evento já foi processado,
                // simplesmente respondemos 200.
                // =============================================

                var eventoExistente =
                    await _dbContext
                        .AsaasWebhookEvents
                        .AsNoTracking()
                        .AnyAsync(
                            x =>
                                x.EventId ==
                                eventId,
                            cancellationToken
                        );


                if (eventoExistente)
                {
                    _logger.LogInformation(
                        "Evento Asaas {EventId} já recebido anteriormente.",
                        eventId
                    );

                    return Ok(
                        new
                        {
                            recebido = true,
                            duplicado = true
                        }
                    );
                }


                // =============================================
                // OBJETO PAYMENT
                // =============================================

                JsonElement pagamento =
                    default;


                var possuiPagamento =
                    raiz.TryGetProperty(
                        "payment",
                        out pagamento
                    ) &&
                    pagamento.ValueKind ==
                    JsonValueKind.Object;


                var paymentId =
                    possuiPagamento
                        ? ObterString(
                            pagamento,
                            "id"
                        )
                        : null;


                var customerId =
                    possuiPagamento
                        ? ObterString(
                            pagamento,
                            "customer"
                        )
                        : null;


                var subscriptionId =
                    possuiPagamento
                        ? ObterString(
                            pagamento,
                            "subscription"
                        )
                        : null;


                var paymentStatus =
                    possuiPagamento
                        ? ObterString(
                            pagamento,
                            "status"
                        )
                        : null;


                var externalReference =
                    possuiPagamento
                        ? ObterString(
                            pagamento,
                            "externalReference"
                        )
                        : null;


                // =============================================
                // CRIAR LOG DO EVENTO
                // =============================================

                var evento =
                    new AsaasWebhookEvent
                    {
                        EventId =
                            eventId,

                        EventType =
                            eventType,

                        PaymentId =
                            paymentId,

                        CustomerId =
                            customerId,

                        SubscriptionId =
                            subscriptionId,

                        PaymentStatus =
                            paymentStatus,

                        ExternalReference =
                            externalReference,

                        PayloadJson =
                            payload,

                        ReceivedAt =
                            DateTime.UtcNow,

                        Processed =
                            false,

                        Success =
                            false
                    };


                _dbContext
                    .AsaasWebhookEvents
                    .Add(
                        evento
                    );


                try
                {
                    await _dbContext
                        .SaveChangesAsync(
                            cancellationToken
                        );
                }
                catch (DbUpdateException)
                {
                    // Pode acontecer se dois envios iguais
                    // chegarem praticamente ao mesmo tempo.

                    return Ok(
                        new
                        {
                            recebido = true,
                            duplicado = true
                        }
                    );
                }


                try
                {
                    // =========================================
                    // LOCALIZAR ASSINATURA
                    // =========================================

                    UserSubscription?
                        assinatura = null;


                    if (
                        !string.IsNullOrWhiteSpace(
                            subscriptionId))
                    {
                        assinatura =
                            await _dbContext
                                .UserSubscriptions
                                .FirstOrDefaultAsync(
                                    x =>
                                        x.AsaasSubscriptionId ==
                                        subscriptionId,
                                    cancellationToken
                                );
                    }


                    if (
                        assinatura == null &&
                        !string.IsNullOrWhiteSpace(
                            customerId))
                    {
                        assinatura =
                            await _dbContext
                                .UserSubscriptions
                                .FirstOrDefaultAsync(
                                    x =>
                                        x.AsaasCustomerId ==
                                        customerId,
                                    cancellationToken
                                );
                    }


                    // =========================================
                    // TENTAR IDENTIFICAR PELO
                    // EXTERNAL REFERENCE
                    // =========================================

                    string? userId =
                        null;


                    if (
                        !string.IsNullOrWhiteSpace(
                            externalReference))
                    {
                        const string prefixo =
                            "CHEFEIA-PREMIUM-";


                        if (
                            externalReference
                                .StartsWith(
                                    prefixo,
                                    StringComparison
                                        .OrdinalIgnoreCase
                                ))
                        {
                            userId =
                                externalReference
                                    .Substring(
                                        prefixo.Length
                                    );
                        }
                    }


                    if (
                        assinatura == null &&
                        !string.IsNullOrWhiteSpace(
                            userId))
                    {
                        assinatura =
                            await _dbContext
                                .UserSubscriptions
                                .FirstOrDefaultAsync(
                                    x =>
                                        x.UserId ==
                                        userId,
                                    cancellationToken
                                );
                    }


                    if (assinatura != null)
                    {
                        userId =
                            assinatura.UserId;


                        if (
                            !string.IsNullOrWhiteSpace(
                                subscriptionId))
                        {
                            assinatura
                                .AsaasSubscriptionId =
                                    subscriptionId;
                        }


                        if (
                            !string.IsNullOrWhiteSpace(
                                paymentId))
                        {
                            assinatura
                                .LastPaymentId =
                                    paymentId;
                        }


                        if (
                            !string.IsNullOrWhiteSpace(
                                customerId))
                        {
                            assinatura
                                .AsaasCustomerId =
                                    customerId;
                        }
                    }


                    evento.UserId =
                        userId;


                    // =========================================
                    // USUÁRIO
                    // =========================================

                    AppUser? usuario =
                        null;


                    if (
                        !string.IsNullOrWhiteSpace(
                            userId))
                    {
                        usuario =
                            await _userManager
                                .FindByIdAsync(
                                    userId
                                );
                    }


                    // =========================================
                    // PROCESSAR EVENTO
                    // =========================================

                    switch (
                        eventType
                            .Trim()
                            .ToUpperInvariant())
                    {
                        // =====================================
                        // COBRANÇA CRIADA
                        // =====================================

                        case "PAYMENT_CREATED":

                            if (assinatura != null)
                            {
                                assinatura.Status =
                                    "PENDING";

                                assinatura.IsActive =
                                    false;
                            }

                            break;


                        // =====================================
                        // PAGAMENTO CONFIRMADO
                        // =====================================

                        case "PAYMENT_CONFIRMED":

                            if (
                                assinatura != null &&
                                usuario != null)
                            {
                                assinatura.Status =
                                    "ACTIVE";

                                assinatura.IsActive =
                                    true;

                                assinatura.ActivatedAt ??=
                                    DateTime.UtcNow;

                                assinatura.LastPaymentAt =
                                    DateTime.UtcNow;


                                usuario.PlanCode =
                                    "PREMIUM";


                                await _userManager
                                    .UpdateAsync(
                                        usuario
                                    );
                            }

                            break;


                        // =====================================
                        // PAGAMENTO RECEBIDO
                        //
                        // Também mantemos o Premium ativo.
                        // =====================================

                        case "PAYMENT_RECEIVED":

                            if (
                                assinatura != null &&
                                usuario != null)
                            {
                                assinatura.Status =
                                    "ACTIVE";

                                assinatura.IsActive =
                                    true;

                                assinatura.ActivatedAt ??=
                                    DateTime.UtcNow;

                                assinatura.LastPaymentAt =
                                    DateTime.UtcNow;


                                usuario.PlanCode =
                                    "PREMIUM";


                                await _userManager
                                    .UpdateAsync(
                                        usuario
                                    );
                            }

                            break;


                        // =====================================
                        // PAGAMENTO VENCIDO
                        // =====================================

                        case "PAYMENT_OVERDUE":

                            if (assinatura != null)
                            {
                                assinatura.Status =
                                    "OVERDUE";

                                assinatura.IsActive =
                                    false;
                            }


                            if (usuario != null)
                            {
                                usuario.PlanCode =
                                    "FREE";


                                await _userManager
                                    .UpdateAsync(
                                        usuario
                                    );
                            }

                            break;


                        // =====================================
                        // ESTORNO
                        // =====================================

                        case "PAYMENT_REFUNDED":

                            if (assinatura != null)
                            {
                                assinatura.Status =
                                    "CANCELED";

                                assinatura.IsActive =
                                    false;

                                assinatura.CanceledAt =
                                    DateTime.UtcNow;
                            }


                            if (usuario != null)
                            {
                                usuario.PlanCode =
                                    "FREE";


                                await _userManager
                                    .UpdateAsync(
                                        usuario
                                    );
                            }

                            break;


                        // =====================================
                        // COBRANÇA REMOVIDA
                        // =====================================

                        case "PAYMENT_DELETED":

                            if (assinatura != null)
                            {
                                assinatura.Status =
                                    "CANCELED";

                                assinatura.IsActive =
                                    false;

                                assinatura.CanceledAt =
                                    DateTime.UtcNow;
                            }


                            if (usuario != null)
                            {
                                usuario.PlanCode =
                                    "FREE";


                                await _userManager
                                    .UpdateAsync(
                                        usuario
                                    );
                            }

                            break;


                        default:

                            _logger.LogInformation(
                                "Evento Asaas {EventType} recebido mas sem ação configurada.",
                                eventType
                            );

                            break;
                    }


                    // =========================================
                    // FINALIZAR EVENTO
                    // =========================================

                    evento.Processed =
                        true;

                    evento.Success =
                        true;

                    evento.ProcessedAt =
                        DateTime.UtcNow;

                    evento.ErrorMessage =
                        null;


                    await _dbContext
                        .SaveChangesAsync(
                            cancellationToken
                        );


                    _logger.LogInformation(
                        "Webhook Asaas processado. Evento {EventId} - {EventType}.",
                        eventId,
                        eventType
                    );


                    return Ok(
                        new
                        {
                            recebido = true
                        }
                    );
                }
                catch (Exception ex)
                {
                    evento.Processed =
                        true;

                    evento.Success =
                        false;

                    evento.ProcessedAt =
                        DateTime.UtcNow;

                    evento.ErrorMessage =
                        ex.Message.Length > 2000
                            ? ex.Message.Substring(
                                0,
                                2000
                            )
                            : ex.Message;


                    try
                    {
                        await _dbContext
                            .SaveChangesAsync(
                                cancellationToken
                            );
                    }
                    catch
                    {
                    }


                    _logger.LogError(
                        ex,
                        "Erro ao processar webhook Asaas {EventId}.",
                        eventId
                    );


                    return StatusCode(
                        StatusCodes
                            .Status500InternalServerError
                    );
                }
            }
        }


        // =====================================================
        // PEGAR STRING DO JSON
        // =====================================================

        private static string? ObterString(
            JsonElement elemento,
            string propriedade)
        {
            if (
                !elemento.TryGetProperty(
                    propriedade,
                    out var valor))
            {
                return null;
            }


            if (
                valor.ValueKind ==
                JsonValueKind.Null ||
                valor.ValueKind ==
                JsonValueKind.Undefined)
            {
                return null;
            }


            if (
                valor.ValueKind ==
                JsonValueKind.String)
            {
                return valor.GetString();
            }


            return valor.ToString();
        }
    }
}