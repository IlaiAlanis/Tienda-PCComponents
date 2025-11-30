
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.PagoDTOs
{
    public class CreatePaymentMethodRequest
    {
        [JsonPropertyName("stripeToken")]
        public string StripeToken { get; set; }

        [JsonPropertyName("titular")]
        [Required(ErrorMessage = "El titular es requerido")]
        public string Titular { get; set; }

        [JsonPropertyName("esPrincipal")]
        public bool EsPrincipal { get; set; }

        [JsonPropertyName("numeroTarjeta")]
        public string NumeroTarjeta { get; set; }

        [JsonPropertyName("fechaVencimiento")]
        public string FechaVencimiento { get; set; }

        [JsonPropertyName("cvv")]
        public string Cvv { get; set; }
    }
}
