namespace API_TI.Models.DTOs.CotizacionDTOs
{
    public class CotizacionEnvioDto
    {
        public string Proveedor { get; set; }
        public string Servicio { get; set; }
        public decimal Costo { get; set; }
        public int DiasEntrega { get; set; }
        public string Descripcion { get; set; }
    }
}
