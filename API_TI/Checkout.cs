using API_TI.Models.dbModels;
using API_TI.Models.DTOs.CarritoDTOs;
using API_TI.Models.DTOs.DireccionDTOs;
using API_TI.Models.DTOs.PagoDTOs;
using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.CheckoutDTOs
{
    public class Checkout
    {
        [JsonPropertyName("usuarioId")]
        public int UsuarioId { get; internal set; }

        [JsonPropertyName("carrito")]
        public CarritoDto Carrito { get; set; }

        [JsonPropertyName("metodoPago")]
        public MetodoPagoDto? MetodoPago { get; set; }

        [JsonPropertyName("direccionEnvio")]
        public DireccionDto DireccionEnvio { get; set; }

        [JsonPropertyName("costoEnvio")]
        public decimal CostoEnvio { get; set; }

        [JsonPropertyName("totalPagar")]
        public decimal TotalPagar => Carrito.Total + CostoEnvio;

    }
}
