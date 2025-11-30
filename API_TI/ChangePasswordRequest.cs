using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.UsuarioDTOs
{
    public class ChangePasswordRequest
    {
        [JsonPropertyName("currentPassword")]
        [Required(ErrorMessage = "La contraseña actual es requerida")]
        public string CurrentPassword { get; set; }

        [JsonPropertyName("newPassword")]
        [Required(ErrorMessage = "La nueva contraseña es requerida")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string NewPassword { get; set; }
    }
}
