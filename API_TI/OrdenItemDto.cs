using API_TI.Models.DTOs.ProductoDTOs;
using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.OrdenDTOs
{
    public class OrdenItemDto
    {
        [JsonPropertyName("idOrdenItem")]
        public int IdOrdenItem { get; set; }

        [JsonPropertyName("productoId")]
        public int ProductoId { get; set; }

        [JsonPropertyName("nombreProducto")]
        public string NombreProducto { get; set; }

        [JsonPropertyName("imagenUrl")]
        public string? ImagenUrl { get; set; }

        [JsonPropertyName("cantidad")]
        public int Cantidad { get; set; }

        [JsonPropertyName("precioUnitario")]
        public decimal PrecioUnitario { get; set; }

        [JsonPropertyName("descuentoAplicado")]
        public decimal? DescuentoAplicado { get; set; }

        [JsonPropertyName("subtotal")]
        public decimal? Subtotal { get; set; }

        [JsonPropertyName("producto")]
        public ProductoSimpleDto? Producto { get; set; }
    }
}
