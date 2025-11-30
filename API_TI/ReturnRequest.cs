namespace API_TI.Models.DTOs.ReembolsoDTOs
{
    public class ReturnRequest
    {
        public int OrdenId { get; set; }
        public string TipoDevolucion { get; set; } // RETURN or EXCHANGE
        public string Motivo { get; set; }
        public List<ReturnItemRequest> Items { get; set; }
    }
}
