using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.UsuarioDTOs
{
    public class UsuarioDto
    {
        [JsonPropertyName("idUsuario")]
        public int IdUsuario { get; set; }

        [JsonPropertyName("nombreUsuario")]
        public string NombreUsuario { get; set; } = null!;
        
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = null!;

        [JsonPropertyName("apellidoPaeterno")]
        public string? ApellidoPaterno { get; set; }

        [JsonPropertyName("apellidoMaterno")]
        public string? ApellidoMaterno { get; set; }

        [JsonPropertyName("correo")]
        public string Correo { get; set; } = null!;

        [JsonPropertyName("rol")]
        public int Rol { get; set; }

        [JsonPropertyName("telefono")]
        public string? Telefono { get; set; }

        [JsonPropertyName("estaActivo")]
        public bool EstaActivo { get; set; }

        [JsonPropertyName("fechaCreacion")]
        public DateTime FechaCreacion { get; set; }
    }
}
