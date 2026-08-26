namespace chefeia.Models
{
    public class ConsultaReceitaIA
    {
        public List<string> Ingredientes { get; set; } = new();

        public string Preferencia { get; set; } = string.Empty;

        public int Porcoes { get; set; } = 1;
    }
}