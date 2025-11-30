using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.OrdenDTOs
{
    public class OrdenDescuentoDto
    {
        [JsonPropertyName("nombreDescuento")]
        public string NombreDescuento { get; set; }
        
        [JsonPropertyName("tipoDescuento")]
        public string TipoDescuento { get; set; } 
        
        [JsonPropertyName("cupon")]
        public string? CodigoCupon { get; set; }

        [JsonPropertyName("montoDescuento")]
        public decimal MontoDescuento { get; set; }

        [JsonPropertyName("montoAplicado")]
        public decimal MontoAplicado => MontoDescuento;
    }
}
