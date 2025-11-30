namespace API_TI.Models.DTOs.PagoDTOs
{
    public class PagoTransaccionDto
    {
        public int IdPago { get; set; }
        public int OrdenId { get; set; }
        public string MetodoPago { get; set; }
        public string Estatus { get; set; }
        public decimal Monto { get; set; }
        public string ReferenciaGateway { get; set; }
        public DateTime FechaTransaccion { get; set; }
    }
}
