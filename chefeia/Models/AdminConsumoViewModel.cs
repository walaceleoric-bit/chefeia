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
        // RAPIDAPI
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