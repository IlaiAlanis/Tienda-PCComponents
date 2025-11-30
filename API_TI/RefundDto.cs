namespace API_TI.Models.DTOs.ReembolsoDTOs
{
    public class RefundDto
    {
        public int IdReembolso { get; set; }
        public int OrdenId { get; set; }
        public decimal Monto { get; set; }
        public string Motivo { get; set; }
        public string Estatus { get; set; }
        public DateTime? FechaSolicitud { get; set; }
    }
}
