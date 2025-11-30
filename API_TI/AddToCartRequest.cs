using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.CarritoDTOs
{
    public class AddToCartRequest
    {
        [JsonPropertyName("productoId")]
        [Required(ErrorMessage = "El ID del producto es requerido")]
        public int ProductoId { get; set; }
        [JsonPropertyName("cantidad")]
        [Required(ErrorMessage = "La cantidad es requerida")]
        public int Cantidad { get; set; }
    }
}
