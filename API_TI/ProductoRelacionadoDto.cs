using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.ProductoDTOs
{
    public class ProductoRelacionadoDto
    {
        [JsonPropertyName("productoId")]
        public int ProductoId { get; set; }

        [JsonPropertyName("productoRelacionadoId")]
        public int ProductoRelacionadoId { get; set; }

        [JsonPropertyName("nombreProducto")]
        public string? NombreProducto { get; set; }

        [JsonPropertyName("nombreRelacionado")]
        public string? NombreRelacionado { get; set; }
    }
}
