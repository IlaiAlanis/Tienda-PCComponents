namespace API_TI.Models.DTOs.InventarioDTOs
{
    public class StockStatusDto
    {
        public int ProductoId { get; set; }
        public int StockTotal { get; set; }
        public int StockReservado { get; set; }
        public int StockDisponible { get; set; }
        public bool SuficienteStock { get; set; }
        public bool EsPreorden { get; set; }
        public DateTime? FechaEstimadaEntrega { get; set; }
    }
}
