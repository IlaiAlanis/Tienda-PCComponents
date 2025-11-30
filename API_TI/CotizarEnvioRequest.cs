using System.ComponentModel.DataAnnotations;

namespace API_TI.Models.DTOs.CotizacionDTOs
{
    public class CotizarEnvioRequest
    {
        [Required] public int DireccionId { get; set; }
        [Required] public decimal PesoKg { get; set; }
        public decimal? ValorDeclarado { get; set; }
    }
}
