using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.CarritoDTOs
{
    public class CarritoDto
    {
        [JsonPropertyName("idCarrito")]
        public int IdCarrito { get; set; }
        
        [JsonPropertyName("usuarioId")]
        public int UsuarioId { get; set; }
       
        [JsonPropertyName("items")]
        public List<CarritoItemDto> Items { get; set; } = new();
        
        [JsonPropertyName("descuentosAplicados")]
        public List<CarritoDescuentoDto> DescuentosAplicados { get; set; } = new(); 
        
        [JsonPropertyName("subtotal")]
        public decimal Subtotal { get; set; }

        [JsonPropertyName("descuentoTotal")]
        public decimal DescuentoTotal { get; set; }

        [JsonPropertyName("impuestoTotal")]
        public decimal ImpuestoTotal { get; set; }

        [JsonPropertyName("envioTotal")]
        public decimal EnvioTotal { get; set; }

        [JsonPropertyName("total")]
        public decimal Total { get; set; }

        [JsonPropertyName("totalItems")]
        public int TotalItems { get; set; }
    }
}
