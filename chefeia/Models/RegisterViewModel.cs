using System.ComponentModel.DataAnnotations;

namespace chefeia.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Informe seu nome.")]
        [StringLength(
            150,
            MinimumLength = 2,
            ErrorMessage = "Informe um nome válido."
        )]
        [Display(Name = "Nome")]
        public string Name { get; set; } = string.Empty;


        [Required(ErrorMessage = "Informe seu e-mail.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "Informe uma senha.")]
        [DataType(DataType.Password)]
        [StringLength(
            100,
            MinimumLength = 6,
            ErrorMessage = "A senha precisa ter pelo menos 6 caracteres."
        )]
        [Display(Name = "Senha")]
        public string Password { get; set; } = string.Empty;


        [Required(ErrorMessage = "Confirme sua senha.")]
        [DataType(DataType.Password)]
        [Compare(
            nameof(Password),
            ErrorMessage = "As senhas não são iguais."
        )]
        [Display(Name = "Confirmar senha")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}