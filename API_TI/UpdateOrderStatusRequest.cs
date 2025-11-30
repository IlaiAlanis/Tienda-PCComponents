using System.ComponentModel.DataAnnotations;

namespace API_TI.Models.DTOs.OrderDTOs
{
    public class UpdateOrderStatusRequest
    {
        [Required]
        public int NuevoEstatusId { get; set; }

        public string? NumeroSeguimiento { get; set; }
        public int? OperadorEnvioId { get; set; }
        public string? Notas { get; set; }
    }
}
