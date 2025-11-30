using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_TI.Models.Auth
{
    public class LoginRequestAuth
    {
        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        [StringLength(255, ErrorMessage = "El correo no puede exceder 255 caracteres")]
        [JsonPropertyName("correo")]  // Map to lowercase
        public string Correo { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
        [JsonPropertyName("contrasena")]  // Map frontend "password" to backend "Contrasena"
        public string Contrasena { get; set; }

        //[JsonPropertyName("recaptchaToken")]
        //public string? RecaptchaToken { get; set; }
    }
}
