using System.ComponentModel.DataAnnotations;

namespace chefeia.Models
{
    public class AdminResetSenhaViewModel
    {
        [Required]
        public string UserId { get; set; } =
            string.Empty;


        public string UserName { get; set; } =
            string.Empty;


        public string Email { get; set; } =
            string.Empty;


        [Required(
            ErrorMessage = "Informe a nova senha."
        )]
        [DataType(DataType.Password)]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage =
                "A senha deve ter pelo menos 6 caracteres."
        )]
        [Display(Name = "Nova senha")]
        public string NewPassword { get; set; } =
            string.Empty;


        [Required(
            ErrorMessage = "Confirme a nova senha."
        )]
        [DataType(DataType.Password)]
        [Compare(
            nameof(NewPassword),
            ErrorMessage =
                "As senhas não são iguais."
        )]
        [Display(Name = "Confirmar nova senha")]
        public string ConfirmPassword { get; set; } =
            string.Empty;
    }
}