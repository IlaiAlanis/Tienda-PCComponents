using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.AdminDTOs
{
    public class AdminUserDto
    {
        [JsonPropertyName("idUsuario")]
        public int IdUsuario { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("apellido")]
        public string Apellido { get; set; } = string.Empty;

        [JsonPropertyName("nombreCompleto")]
        public string NombreCompleto { get; set; } = string.Empty;

        [JsonPropertyName("correo")]
        public string Correo { get; set; }

        [JsonPropertyName("rol")]
        public string Rol { get; set; }

        [JsonPropertyName("estaActivo")]
        public bool EstaActivo { get; set; }

        [JsonPropertyName("correoVerificado")]
        public bool CorreoVerificado { get; set; }

        [JsonPropertyName("fechaCreacion")]
        public DateTime FechaCreacion { get; set; }

        [JsonPropertyName("ultimoLogin")]
        public DateTime? UltimoLogin { get; set; }

        [JsonPropertyName("intentosFallidos")]
        public int IntentosFallidos { get; set; }

        [JsonPropertyName("totalPedidos")]
        public int TotalPedidos { get; set; }
    }
}
