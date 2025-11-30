namespace API_TI.Models.DTOs.ReporteDTOs
{
    public class InventarioReporteDto
    {
        public int TotalProductos { get; set; }
        public int ProductosBajoStock { get; set; }
        public int ProductosAgotados { get; set; }
        public decimal ValorInventarioTotal { get; set; }
        public List<ProductoBajoStockDto> ProductosCriticos { get; set; }
    }
}
