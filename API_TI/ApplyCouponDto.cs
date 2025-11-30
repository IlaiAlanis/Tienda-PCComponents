using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.CarritoDTOs
{
    public class ApplyCouponDto
    {
        [JsonPropertyName("codigoCupon")]
        [Required(ErrorMessage = "El código de cupón es requerido")]
        public string CodigoCupon { get; set; } = string.Empty;
    }
}
