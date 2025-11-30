namespace API_TI.Models.DTOs.CotizacionDTOs
{
    public class CotizacionesEnvioResponse
    {
        public List<CotizacionEnvioDto> Cotizaciones { get; set; }
        public string DireccionDestino { get; set; }
    }
}
