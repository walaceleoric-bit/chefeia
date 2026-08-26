using System.ComponentModel.DataAnnotations;

namespace chefeia.Models
{
    public class AsaasWebhookEvent
    {
        public int Id { get; set; }


        // =====================================================
        // IDENTIFICAÇÃO DO EVENTO ASAAS
        // =====================================================

        [Required]
        [MaxLength(150)]
        public string EventId { get; set; } =
            string.Empty;


        // Exemplo:
        //
        // PAYMENT_CREATED
        // PAYMENT_CONFIRMED
        // PAYMENT_RECEIVED
        // PAYMENT_OVERDUE
        // PAYMENT_REFUNDED
        // PAYMENT_DELETED

        [Required]
        [MaxLength(100)]
        public string EventType { get; set; } =
            string.Empty;


        // =====================================================
        // COBRANÇA
        // =====================================================

        [MaxLength(150)]
        public string? PaymentId { get; set; }


        [MaxLength(150)]
        public string? CustomerId { get; set; }


        [MaxLength(150)]
        public string? SubscriptionId { get; set; }


        [MaxLength(100)]
        public string? PaymentStatus { get; set; }


        // =====================================================
        // REFERÊNCIA AO NOSSO USUÁRIO
        // =====================================================

        [MaxLength(500)]
        public string? ExternalReference { get; set; }


        [MaxLength(450)]
        public string? UserId { get; set; }


        // =====================================================
        // PROCESSAMENTO
        // =====================================================

        public bool Processed { get; set; }


        public bool Success { get; set; }


        [MaxLength(2000)]
        public string? ErrorMessage { get; set; }


        // =====================================================
        // PAYLOAD ORIGINAL
        //
        // Guardamos o JSON recebido para auditoria e diagnóstico.
        // =====================================================

        public string PayloadJson { get; set; } =
            string.Empty;


        // =====================================================
        // DATAS
        // =====================================================

        public DateTime ReceivedAt { get; set; } =
            DateTime.UtcNow;


        public DateTime? ProcessedAt { get; set; }
    }
}