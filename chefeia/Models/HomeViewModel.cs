namespace chefeia.Models
{
    public class HomeViewModel
    {
        public SiteSettings Settings { get; set; } =
            new SiteSettings();

        public IEnumerable<Receita> Receitas { get; set; } =
            new List<Receita>();
    }
}