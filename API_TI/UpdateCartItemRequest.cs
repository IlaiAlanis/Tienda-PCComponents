using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.CarritoDTOs
{
    public class UpdateCartItemRequest
    {
        [JsonPropertyName("cantidad")]
        [Required(ErrorMessage = "La cantidad es requerida")]
        public int Cantidad { get; set; }

    }
}
