using System.ComponentModel.DataAnnotations;

namespace API_TI.Models.DTOs.PagoDTOs
{
    public class CapturePayPalOrderRequest
    {
        [Required] public int OrdenId { get; set; }
        [Required] public string PayPalOrderId { get; set; }
    }
}
