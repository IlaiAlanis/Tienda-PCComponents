using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.ProductoDTOs
{
    public class ProductoResenaDto
    {
        [JsonPropertyName("idResena")]
        public int IdResena { get; set; }
        [JsonPropertyName("nombreUsuario")]
        public string NombreUsuario { get; set; } = string.Empty;
        [JsonPropertyName("calificacion")]
        public int Calificacion { get; set; }
        [JsonPropertyName("comentario")]
        public string? Comentario { get; set; }
        [JsonPropertyName("fechaCreacion")]
        public DateTime FechaCreacion { get; set; }
    }
}
