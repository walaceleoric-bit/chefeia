using System.ComponentModel.DataAnnotations;

namespace chefeia.Models
{
    public class UserSubscription
    {
        public int Id { get; set; }


        // =====================================================
        // USUÁRIO
        // =====================================================

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } =
            string.Empty;

        public AppUser? User { get; set; }


        // =====================================================
        // ASAAS
        // =====================================================

        // ID do cliente dentro do Asaas.
        [MaxLength(100)]
        public string? AsaasCustomerId { get; set; }


        // ID da assinatura criada no Asaas.
        [MaxLength(100)]
        public string? AsaasSubscriptionId { get; set; }


        // ID da última cobrança conhecida.
        [MaxLength(100)]
        public string? LastPaymentId { get; set; }


        // =====================================================
        // PLANO
        // =====================================================

        [Required]
        [MaxLength(30)]
        public string PlanCode { get; set; } =
            "PREMIUM";


        // Valor contratado.
        //
        // Guardamos o valor da assinatura no momento
        // da contratação. Assim, se o Admin mudar
        // o preço depois, não alteramos silenciosamente
        // uma assinatura já existente.
        public decimal Price { get; set; }


        // =====================================================
        // STATUS
        // =====================================================

        // Exemplos internos:
        //
        // PENDING
        // ACTIVE
        // OVERDUE
        // CANCELED
        // EXPIRED

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } =
            "PENDING";


        // =====================================================
        // DATAS
        // =====================================================

        public DateTime CreatedAt { get; set; } =
            DateTime.UtcNow;


        public DateTime? ActivatedAt { get; set; }


        public DateTime? CanceledAt { get; set; }


        // Próximo vencimento informado pelo Asaas.
        public DateTime? NextDueDate { get; set; }


        // Último pagamento confirmado.
        public DateTime? LastPaymentAt { get; set; }


        // =====================================================
        // CONTROLE
        // =====================================================

        public bool IsActive { get; set; }


        // Usaremos depois para guardar o tipo
        // de cobrança escolhido:
        //
        // PIX
        // CREDIT_CARD
        // BOLETO

        [MaxLength(30)]
        public string? BillingType { get; set; }
    }
}