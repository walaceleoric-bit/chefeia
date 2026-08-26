namespace chefeia.Models
{
    public class ReceitaIA
    {
        public string Nome { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        public string Pais { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        public int Porcoes { get; set; }

        public int TempoMinutos { get; set; }

        public List<string> Ingredientes { get; set; } = new();

        public List<string> Passos { get; set; } = new();
    }
}