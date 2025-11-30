using System.ComponentModel.DataAnnotations;

namespace API_TI.Models.DTOs.ReembolsoDTOs
{
    public class RefundRequest
    {
        [Required] public int OrdenId { get; set; }
        [Required] public string Motivo { get; set; }
        public bool RestockItems { get; set; } = true;
    }
}
