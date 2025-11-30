namespace API_TI.Models.DTOs.InventarioDTOs
{
    public class AdjustStockRequest
    {
        public int ProductoId {  get; set; }
        public int TipoMovimientoId {  get; set; }
        public int Cantidad { get; set; }
        public int StockAnterior { get; set; }
        public int StockNuevo { get; set; }
        public string?  Referencia { get; set; }
        public DateTime FechaMovimiento { get; set; }
    }
}
