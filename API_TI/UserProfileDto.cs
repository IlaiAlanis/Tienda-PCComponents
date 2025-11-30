using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.UsuarioDTOs
{
    public class UserProfileDto
    {
        [JsonPropertyName("idUsuario")]
        public int IdUsuario { get; set; }

        [JsonPropertyName("nombreUsuario")]
        public string NombreUsuario { get; set; } = string.Empty;
        
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("apellidoPaterno")]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [JsonPropertyName("apellidoMaterno")]
        public string ApellidoMaterno { get; set; }

        [JsonPropertyName("correo")]
        public string Correo { get; set; } = string.Empty;

        [JsonPropertyName("correoVerificado")]
        public bool CorreoVerificado { get; set; }

        [JsonPropertyName("fechaNacimiento")]
        public DateOnly? FechaNacimiento { get; set; }

        [JsonPropertyName("rol")]
        public string Rol { get; set; }

        [JsonPropertyName("fechaCreacion")]
        public DateTime FechaCreacion { get; set; }

        [JsonPropertyName("ultimoLogin")]
        public DateTime? UltimoLogin { get; set; }

        [JsonPropertyName("avatarUrl")]
        public string? AvatarUrl { get; set; }
    }
}
