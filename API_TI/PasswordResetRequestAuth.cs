using System.ComponentModel.DataAnnotations;

namespace API_TI.Models.Auth
{
    public class PasswordResetRequestAuth
    {
        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        [StringLength(255)]
        public string Email { get; set; }
    }
}
