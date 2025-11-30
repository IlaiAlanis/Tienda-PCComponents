using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.CarritoDTOs
{
    public class CarritoDescuentoDto
    {

        [JsonPropertyName("idDescuento")]
        public int IdDescuento { get; set; }

        [JsonPropertyName("nombreDescuento")]
        public string NombreDescuento { get; set; }

        [JsonPropertyName("tipoDescuento")]
        public string TipoDescuento { get; set; }

        [JsonPropertyName("montoDescuento")]
        public decimal MontoDescuento { get; set; }

        [JsonPropertyName("codigoCupon")]
        public string CodigoCupon { get; set; }
    }
}
