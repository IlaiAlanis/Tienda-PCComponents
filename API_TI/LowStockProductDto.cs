using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.AdminDTOs
{
    public class LowStockProductDto
    {
        [JsonPropertyName("idProducto")]
        public int IdProducto { get; set; }

        [JsonPropertyName("nombreProducto")]
        public string NombreProducto { get; set; }

        [JsonPropertyName("stock")]
        public int Stock { get; set; }

        [JsonPropertyName("stockMinimo")]
        public int StockMinimo { get; set; }
    }
}
