namespace API_TI.Models.DTOs.FacturaDTOs
{
    public class FacturaDto
    {
        public int IdFactura { get; set; }
        public int OrdenId { get; set; }
        public string NumeroOrden { get; set; }
        public string TipoFactura { get; set; }
        public string Serie { get; set; }
        public int? Folio { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuestos { get; set; }
        public decimal Total { get; set; }
        public string Uuid { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaCancelacion { get; set; }

    }
}
