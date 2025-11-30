using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.CarritoDTOs
{
    public class CarritoItemDto
    {
        [JsonPropertyName("idCarritoItem")]
        public int IdCarritoItem { get; set; }

        [JsonPropertyName("productoId")]
        public int ProductoId { get; set; }

        [JsonPropertyName("nombre")]
        public string NombreProducto { get; set; } = string.Empty;

        [JsonPropertyName("imagenUrl")]
        public string ImagenUrl { get; set; }

        [JsonPropertyName("cantidad")]
        public int Cantidad { get; set; }

        [JsonPropertyName("precioUnitario")]
        public decimal PrecioUnitario { get; set; }

        [JsonPropertyName("descuentoAplicado")]
        public decimal DescuentoAplicado { get; set; }

        [JsonPropertyName("subtotal")]
        public decimal Subtotal { get; set; }

        [JsonPropertyName("stockDisponible")]
        public int StockDisponible { get; set; }
    }
}
