using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.AdminDTOs
{
    public class RecentOrderDto
    {
        [JsonPropertyName("idOrden")]
        public int IdOrden { get; set; }

        [JsonPropertyName("nombreCliente")]
        public string NombreCliente { get; set; }

        [JsonPropertyName("fechaCreacion")]
        public DateTime FechaCreacion { get; set; }

        [JsonPropertyName("total")]
        public decimal Total { get; set; }

        [JsonPropertyName("estado")]
        public string Estado { get; set; }
    }
}
