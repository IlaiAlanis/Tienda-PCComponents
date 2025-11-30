namespace API_TI.Models.DTOs.ReporteDTOs
{
    public class ProductoBajoStockDto
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; }
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
    }
}
