namespace chefeia.Models
{
    public class AdminConsumoViewModel
    {
        // =====================================================
        // RESUMO
        // =====================================================

        public int ConsultasHoje { get; set; }

        public int ConsultasMes { get; set; }

        public int ConsultasTotal { get; set; }

        public int SucessosMes { get; set; }

        public int ErrosMes { get; set; }

        public double TempoMedioMs { get; set; }


        // =====================================================
        // RAPIDAPI - CONTROLE INTERNO
        // =====================================================

        public int RapidApiMonthlyRequestLimit { get; set; }

        public int RapidApiRequestsUsed { get; set; }

        public int RapidApiRequestsRemaining { get; set; }

        public double RapidApiRequestsPercentUsed { get; set; }


        // =====================================================
        // RAPIDAPI - LIMITES INFORMADOS PELA API
        // =====================================================

        public int? RequestsLimit { get; set; }

        public int? RequestsRemaining { get; set; }

        public int? CreditLimit { get; set; }

        public int? CreditRemaining { get; set; }


        // =====================================================
        // ÚLTIMAS CONSULTAS
        // =====================================================

        public List<AiUsage> UltimasConsultas { get; set; } =
            new();
    }
}