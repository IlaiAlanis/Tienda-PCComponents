using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.CheckoutDTOs
{
    public class InitiateCheckoutRequest
    {
        [JsonPropertyName("direccionEnvioId")]
        [Required(ErrorMessage = "La dirección de envío es requerida")]
        public int DireccionEnvioId { get; set; }
    }
}
