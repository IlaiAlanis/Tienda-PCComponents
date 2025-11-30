using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.PagoDTOs
{
    public class UpdatePaymentMethodRequest
    {
        [JsonPropertyName("titular")]
        public string Titular { get; set; }

        [JsonPropertyName("fechaVencimiento")]
        public string FechaVencimiento { get; set; }

        [JsonPropertyName("esPrincipal")]
        public bool? EsPrincipal { get; set; }
    }
}
