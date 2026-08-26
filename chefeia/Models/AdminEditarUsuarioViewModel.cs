using System.ComponentModel.DataAnnotations;

namespace chefeia.Models
{
    public class AdminEditarUsuarioViewModel
    {
        [Required]
        public string Id { get; set; } =
            string.Empty;


        [Required(ErrorMessage = "Informe o nome.")]
        [Display(Name = "Nome")]
        public string Name { get; set; } =
            string.Empty;


        [Required(ErrorMessage = "Informe o e-mail.")]
        [EmailAddress(
            ErrorMessage = "Informe um e-mail válido."
        )]
        [Display(Name = "E-mail")]
        public string Email { get; set; } =
            string.Empty;


        [Required]
        [Display(Name = "Plano")]
        public string PlanCode { get; set; } =
            "FREE";


        [Display(Name = "Conta ativa")]
        public bool IsActive { get; set; }


        public bool IsAdmin { get; set; }
    }
}