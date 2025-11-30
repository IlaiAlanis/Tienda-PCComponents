using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.UsuarioDTOs
{
    public class UpdateProfileRequest
    {
        [JsonPropertyName("nombre")]
        [MaxLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres")]
        public string? Nombre { get; set; }

        [JsonPropertyName("apellidoPaterno")]
        [MaxLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
        public string ApellidoPaterno { get; set; }

        [JsonPropertyName("apellidoMaterno")]
        [MaxLength(100, ErrorMessage = "El apellido materno no puede exceder 100 caracteres")]
        public string ApellidoMaterno { get; set; }

        [JsonPropertyName("correo")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string? Correo { get; set; }

        [JsonPropertyName("telefono")]
        [Phone(ErrorMessage = "Teléfono inválido")]
        public string? Telefono { get; set; }

        [JsonPropertyName("fechaNacimiento")]
        public DateOnly? FechaNacimiento { get; set; }

        [JsonPropertyName("currentPassword")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string? CurrentPassword { get; set; }

        [JsonPropertyName("newPassword")]
        [MinLength(8, ErrorMessage = "La nueva contraseña debe tener al menos 8 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#._\-+(){}[\]=/\\|~^,;:])[A-Za-z\d@$!%*?&#._\-+(){}[\]=/\\|~^,;:]{8,}$",
            ErrorMessage = "La contraseña debe contener mayúscula, minúscula, número y un carácter especial permitido.")]
        public string? NewPassword { get; set; }

    }
}
