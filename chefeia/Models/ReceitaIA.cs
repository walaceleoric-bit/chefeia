namespace chefeia.Models
{
    public class ReceitaIA
    {
        // =====================================================
        // RESULTADO DA ANÁLISE DA IA
        // =====================================================

        // Valores esperados:
        // RECEITA
        // SUGESTAO
        // INSUFICIENTE
        public string TipoResposta { get; set; } = "RECEITA";

        // Mensagem usada quando a IA decidir não gerar
        // uma receita imediatamente.
        public string Mensagem { get; set; } = string.Empty;

        // Sugestões de ingredientes ou alternativas
        // que podem melhorar/viabilizar a receita.
        public List<string> Sugestoes { get; set; } = new();


        // =====================================================
        // RECEITA
        // Preenchido quando TipoResposta = RECEITA
        // =====================================================

        public string Nome { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        public string Pais { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        public int Porcoes { get; set; }

        public int TempoMinutos { get; set; }

        public List<string> Ingredientes { get; set; } = new();

        public List<string> Passos { get; set; } = new();


        // =====================================================
        // FACILITADORES
        // =====================================================

        public bool TemReceita =>
            TipoResposta.Equals(
                "RECEITA",
                StringComparison.OrdinalIgnoreCase
            );

        public bool PrecisaDeMaisIngredientes =>
            TipoResposta.Equals(
                "INSUFICIENTE",
                StringComparison.OrdinalIgnoreCase
            );

        public bool TemSugestao =>
            TipoResposta.Equals(
                "SUGESTAO",
                StringComparison.OrdinalIgnoreCase
            );
    }
}