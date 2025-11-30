using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.PagoDTOs
{
    public class PaymentMethodDto
    {
        [JsonPropertyName("idMetodoPago")]
        public int IdMetodoPago { get; set; }

        [JsonPropertyName("numeroTarjeta")]
        public string NumeroTarjeta { get; set; }

        [JsonPropertyName("ultimos4")]
        public string Ultimos4 { get; set; }

        [JsonPropertyName("marca")]
        public string Marca { get; set; }

        [JsonPropertyName("titular")]
        public string Titular { get; set; }

        [JsonPropertyName("fechaVencimiento")]
        public string FechaVencimiento { get; set; }

        [JsonPropertyName("esPrincipal")]
        public bool EsPrincipal { get; set; }

        [JsonPropertyName("stripePaymentMethodId")]
        public string StripePaymentMethodId { get; set; }

        [JsonPropertyName("fechaCreacion")]
        public DateTime FechaCreacion { get; set; }

    }
}
