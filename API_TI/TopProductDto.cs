using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.AdminDTOs
{
    public class TopProductDto
    {
        [JsonPropertyName("productoId")]
        public int ProductoId { get; set; }
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }
        [JsonPropertyName("cantidadVendida")]
        public int CantidadVendida { get; set; }
        [JsonPropertyName("totalVentas")]
        public decimal TotalVentas { get; set; }
        [JsonPropertyName("precioBase")]
        public decimal PrecioBase { get; set; }
    }
}
