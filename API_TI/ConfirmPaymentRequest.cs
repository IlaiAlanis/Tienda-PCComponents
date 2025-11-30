using System.ComponentModel.DataAnnotations;

namespace API_TI.Models.DTOs.PagoDTOs
{
    public class ConfirmPaymentRequest
    {
        [Required] public int OrdenId { get; set; }
        [Required] public string PaymentIntentId { get; set; }
    }
}
