using chefeia.Models;

namespace chefeia.Services
{
    public interface IReceitaService
    {
        IEnumerable<Receita> ObterDestaques();

        IEnumerable<Receita> Buscar(string termo);
    }
}