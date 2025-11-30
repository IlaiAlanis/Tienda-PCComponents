using System.ComponentModel.DataAnnotations;

namespace API_TI.Models.DTOs.FacturaDTOs
{
    public class CreateFacturaRequest
    {
        [Required] public int OrdenId { get; set; }
        [Required] public string TipoFactura { get; set; } // INGRESO, EGRESO
        public string RfcReceptor { get; set; }
        public string RazonSocial { get; set; }

    }
}
