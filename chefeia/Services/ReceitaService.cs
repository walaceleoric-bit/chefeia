using chefeia.Models;

namespace chefeia.Services
{
    public class ReceitaService : IReceitaService
    {
        private readonly List<Receita> _receitas =
        [
            new Receita
            {
                Id = 1,
                Nome = "Brigadeiro",
                Descricao = "Clássico doce brasileiro feito com leite condensado e chocolate.",
                Pais = "Brasil",
                Bandeira = "🇧🇷",
                Categoria = "Doces",
                ImagemUrl = "https://placehold.co/600x400?text=Brigadeiro",
                TempoPreparoMinutos = 25,
                Dificuldade = "Fácil"
            },

            new Receita
            {
                Id = 2,
                Nome = "Tiramisu",
                Descricao = "Sobremesa italiana cremosa preparada com café.",
                Pais = "Itália",
                Bandeira = "🇮🇹",
                Categoria = "Sobremesas",
                ImagemUrl = "https://placehold.co/600x400?text=Tiramisu",
                TempoPreparoMinutos = 40,
                Dificuldade = "Médio"
            },

            new Receita
            {
                Id = 3,
                Nome = "Pastel",
                Descricao = "Massa crocante recheada, muito popular no Brasil.",
                Pais = "Brasil",
                Bandeira = "🇧🇷",
                Categoria = "Salgados",
                ImagemUrl = "https://placehold.co/600x400?text=Pastel",
                TempoPreparoMinutos = 35,
                Dificuldade = "Fácil"
            }
        ];

        public IEnumerable<Receita> ObterDestaques()
        {
            return _receitas;
        }

        public IEnumerable<Receita> Buscar(string termo)
        {
            if (string.IsNullOrWhiteSpace(termo))
            {
                return _receitas;
            }

            return _receitas.Where(r =>
                r.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                r.Descricao.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                r.Categoria.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                r.Pais.Contains(termo, StringComparison.OrdinalIgnoreCase)
            );
        }
    }
}