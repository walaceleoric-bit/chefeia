namespace chefeia.Models
{
    public class Receita
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        public string Pais { get; set; } = string.Empty;

        public string Bandeira { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        public string ImagemUrl { get; set; } = string.Empty;

        public int TempoPreparoMinutos { get; set; }

        public string Dificuldade { get; set; } = string.Empty;
    }
}