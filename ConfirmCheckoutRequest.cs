using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.CheckoutDTOs
{
    public class ConfirmCheckoutRequest
    {
        [JsonPropertyName("direccionEnvioId")]
        [Required(ErrorMessage = "La dirección de envío es requerida")]
        public int DireccionEnvioId { get; set; }

        [JsonPropertyName("metodoPagoId")]
        [Required(ErrorMessage = "El método de pago es requerido")]
        public string MetodoPago { get; set; } // "stripe" or "paypal"

        [JsonPropertyName("metodoPago")]
        public string? TipoPago { get; set; }

        [JsonPropertyName("paymentIntentId")]
        public string? PaymentIntentId { get; set; } // For Stripe

        [JsonPropertyName("payPalOrderId")]
        public string PayPalOrderId { get; set; } // For PayPal

        [JsonPropertyName("notas")]
        [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
        public string? Notas { get; set; }

    }
}
