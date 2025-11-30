using System.ComponentModel.DataAnnotations;

namespace API_TI.Models.Auth
{
    public class VerifyEmailRequestAuth
    {
        [Required(ErrorMessage = "El código de verificación es requerido")]
        [StringLength(10, MinimumLength = 6, ErrorMessage = "El código debe tener entre 6 y 10 caracteres")]
        public string Code { get; set; }
    }
}
