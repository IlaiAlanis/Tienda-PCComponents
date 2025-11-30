namespace API_TI.Models.DTOs.ReembolsoDTOs
{
    public class DevolucionDto
    {
        public int IdDevolucion { get; set; }
        public int OrdenId { get; set; }
        public string TipoDevolucion { get; set; }
        public string Estatus { get; set; }
    }
}
