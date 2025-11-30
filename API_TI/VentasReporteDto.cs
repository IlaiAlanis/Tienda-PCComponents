namespace API_TI.Models.DTOs.ReporteDTOs
{
    public class VentasReporteDto
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int TotalOrdenes { get; set; }
        public decimal VentasTotales { get; set; }
        public decimal PromedioOrden { get; set; }
        public List<VentaPorDiaDto> VentasPorDia { get; set; }
        public List<ProductoMasVendidoDto> ProductosMasVendidos { get; set; }
    }
}
