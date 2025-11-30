using API_TI.Models.DTOs.CarritoDTOs;
using API_TI.Models.DTOs.CotizacionDTOs;
using API_TI.Models.DTOs.DireccionDTOs;
using API_TI.Models.DTOs.ImpuestoDTOs;
using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.CheckoutDTOs
{
    public class CheckoutSummaryDto
    {
        [JsonPropertyName("carrito")]
        public CarritoDto Carrito { get; set; }

        [JsonPropertyName("direccionEnvio")]
        public DireccionDto DireccionEnvio { get; set; }

        [JsonPropertyName("opcionesEnvio")]
        public List<CotizacionEnvioDto> OpcionesEnvio { get; set; }

        [JsonPropertyName("impuesto")]
        public ImpuestoDto Impuesto { get; set; }

        [JsonPropertyName("costos")]
        public ResumenCostosDto Costos { get; set; }
    }
}
