using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.ProductoDTOs
{
    public class ProductoImagenDto
    {
        [JsonPropertyName("idImagen")]
        public int IdImagen { get; set; }
        [JsonPropertyName("productoId")]
        public int ProductoId { get; set; }
        [JsonPropertyName("urlImagen")]
        public string UrlImagen { get; set; } = string.Empty;
        [JsonPropertyName("esPrincipal")]
        public bool EsPrincipal { get; set; }
        [JsonPropertyName("orden")]
        public int? Orden { get; set; }
    }
}
