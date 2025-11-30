using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.UsuarioDTOs
{
    public class UpdateEmailRequest
    {
        [JsonPropertyName("newEmail")]
        [Required(ErrorMessage = "El nuevo email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string NewEmail { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Password { get; set; } = string.Empty;
    }
}
