namespace API_TI.Models.DTOs.ReembolsoDTOs
{
    public class ReturnItemRequest
    {
        public int OrdenItemId { get; set; }
        public int Cantidad { get; set; }
        public int? ProductoIntercambioId { get; set; }
    }
}
