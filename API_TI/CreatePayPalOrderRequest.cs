using System.ComponentModel.DataAnnotations;

namespace API_TI.Models.DTOs.PagoDTOs
{
    public class CreatePayPalOrderRequest
    {
        [Required] public int OrdenId { get; set; }
        [Required] public decimal Monto { get; set; }
    }
}
