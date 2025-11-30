using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.CheckoutDTOs
{
    public class ResumenCostosDto
    {
        [JsonPropertyName("subtotal")]
        public decimal Subtotal { get; set; }

        [JsonPropertyName("descuentoTotal")]
        public decimal DescuentoTotal { get; set; }

        [JsonPropertyName("subtotalConDescuento")]
        public decimal SubtotalConDescuento { get; set; }

        [JsonPropertyName("impuesto")]
        public decimal Impuesto { get; set; }

        [JsonPropertyName("envio")]
        public decimal Envio { get; set; }

        [JsonPropertyName("total")]
        public decimal Total { get; set; }
    }
}
