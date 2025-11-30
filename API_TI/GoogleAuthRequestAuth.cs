using System.ComponentModel.DataAnnotations;

namespace API_TI.Models.Auth
{
    public class GoogleAuthRequestAuth
    {
        [Required(ErrorMessage = "El token de Google es requerido")]
        public string IdToken { get; set; } = null!;
    }
}
