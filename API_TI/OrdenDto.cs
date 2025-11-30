using API_TI.Models.DTOs.DireccionDTOs;
using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.OrdenDTOs
{
    public class OrdenDto
    {
        [JsonPropertyName("idOrden")]
        public int IdOrden { get; set; }

        [JsonPropertyName("numeroOrden")]
        public string NumeroOrden { get; set; }

        [JsonPropertyName("fechaOrden")]
        public DateTime FechaOrden { get; set; }

        [JsonPropertyName("fechaPedido")] 
        public DateTime FechaPedido => FechaOrden;

        [JsonPropertyName("fechaCreacion")] // Another alias
        public DateTime FechaCreacion => FechaOrden;

        [JsonPropertyName("fechaActualizacion")]
        public DateTime? FechaActualizacion { get; set; }


        [JsonPropertyName("estado")]
        public string Estado { get; set; }

        [JsonPropertyName("estatusVenta")] // Keep for compatibility
        public string EstatusVenta => Estado;

        [JsonPropertyName("nombreCliente")]
        public string? NombreCliente { get; set; }

        [JsonPropertyName("emailCliente")]
        public string? EmailCliente { get; set; }

        [JsonPropertyName("telefonoCliente")]
        public string? TelefonoCliente { get; set; }

        [JsonPropertyName("direccionEnvio")]
        public DireccionDto? DireccionEnvio { get; set; }

        [JsonPropertyName("items")]
        public List<OrdenItemDto>? Items { get; set; }

        [JsonPropertyName("descuentos")]
        public List<OrdenDescuentoDto>? Descuentos { get; set; }

        [JsonPropertyName("subtotal")]
        public decimal Subtotal { get; set; }

        [JsonPropertyName("descuento")]
        public decimal Descuento { get; set; }

        [JsonPropertyName("descuentoTotal")]
        public decimal DescuentoTotal => Descuento;

        [JsonPropertyName("impuestos")]
        public decimal Impuestos { get; set; }

        [JsonPropertyName("impuestoTotal")]
        public decimal ImpuestoTotal => Impuestos;

        [JsonPropertyName("costoEnvio")]
        public decimal CostoEnvio { get; set; }

        [JsonPropertyName("envioTotal")]
        public decimal EnvioTotal => CostoEnvio;

        [JsonPropertyName("metodoPago")]
        public string MetodoPago { get; set; }

        [JsonPropertyName("tracking")]
        public string Tracking { get; set; }

        [JsonPropertyName("total")]
        public decimal Total { get; set; }
    }
}
