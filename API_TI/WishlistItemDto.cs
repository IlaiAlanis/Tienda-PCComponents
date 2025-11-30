using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.WishlistDTOs
{
    public class WishlistItemDto
    {
        [JsonPropertyName("idItem")]
        public int IdItem { get; set; }

        [JsonPropertyName("productoId")]
        public int ProductoId { get; set; }

        [JsonPropertyName("nombre")]
        public string NombreProducto { get; set; }

        [JsonPropertyName("imagenUrl")]
        public string ImagenUrl { get; set; }

        [JsonPropertyName("precio")]
        public decimal Precio { get; set; }

        [JsonPropertyName("enStock")]
        public bool EnStock { get; set; }

        [JsonPropertyName("fechaAgregado")]
        public DateTime FechaAgregado { get; set; }
    }
}
