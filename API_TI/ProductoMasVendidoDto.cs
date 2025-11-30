namespace API_TI.Models.DTOs.ReporteDTOs
{
    public class ProductoMasVendidoDto
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; }
        public int CantidadVendida { get; set; }
        public decimal TotalVentas { get; set; }
    }
}
