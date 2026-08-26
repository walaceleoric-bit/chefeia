namespace chefeia.Models
{
    public class AiUsage
    {
        public int Id { get; set; }

        // Data e hora em que a consulta foi realizada
        public DateTime CreatedAt { get; set; } =
            DateTime.UtcNow;

        // Se a consulta terminou com sucesso
        public bool Success { get; set; }

        // Código HTTP retornado pela API
        // Exemplo: 200, 429, 500...
        public int? StatusCode { get; set; }

        // Tempo que a API levou para responder
        public long DurationMs { get; set; }

        // Quantidade de ingredientes enviados
        public int IngredientCount { get; set; }

        // Quantidade de porções solicitadas
        public int Servings { get; set; }

        // Preferência informada
        public string Preference { get; set; } =
            string.Empty;

        // =====================================================
        // RAPIDAPI - REQUESTS
        // =====================================================

        public int? RequestsLimit { get; set; }

        public int? RequestsRemaining { get; set; }


        // =====================================================
        // RAPIDAPI - CRÉDITOS
        // =====================================================

        public int? CreditLimit { get; set; }

        public int? CreditRemaining { get; set; }


        // =====================================================
        // ERRO
        // =====================================================

        // Caso a consulta falhe, guardamos uma descrição.
        public string? ErrorMessage { get; set; }


        // =====================================================
        // USUÁRIO
        // =====================================================

        // Vamos preencher quando criarmos o sistema de login.
        // Por enquanto ficará vazio.
        public string? UserId { get; set; }


        // =====================================================
        // PLANO
        // =====================================================

        // Também será preenchido depois:
        // Free / Premium
        public string? PlanName { get; set; }
    }
}