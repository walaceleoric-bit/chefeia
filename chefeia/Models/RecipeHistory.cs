using System.ComponentModel.DataAnnotations;

namespace chefeia.Models
{
    public class RecipeHistory
    {
        public int Id { get; set; }


        // =====================================================
        // USUÁRIO
        // =====================================================

        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;


        // =====================================================
        // DADOS DA RECEITA
        // =====================================================

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;


        [MaxLength(100)]
        public string Country { get; set; } = string.Empty;


        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;


        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;


        public int PreparationMinutes { get; set; }


        public int Servings { get; set; }


        // =====================================================
        // INGREDIENTES E MODO DE PREPARO
        //
        // Vamos salvar como JSON.
        // Assim não precisamos criar várias tabelas agora.
        // =====================================================

        public string IngredientsJson { get; set; } = "[]";


        public string StepsJson { get; set; } = "[]";


        // =====================================================
        // INFORMAÇÕES DA CONSULTA
        // =====================================================

        [MaxLength(1000)]
        public string RequestedIngredients { get; set; } =
            string.Empty;


        [MaxLength(150)]
        public string Preference { get; set; } =
            string.Empty;


        // =====================================================
        // DATA
        // =====================================================

        public DateTime CreatedAt { get; set; } =
            DateTime.UtcNow;


        // =====================================================
        // RELACIONAMENTO COM USUÁRIO
        // =====================================================

        public AppUser? User { get; set; }
    }
}