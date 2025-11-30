using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.DescuentoDTOs
{
    public class DescuentoDto
    {
        [JsonPropertyName("idDescuento")]
        public int IdDescuento { get; set; }

        [JsonPropertyName("nombreDescuento")]
        public string NombreDescuento { get; set; }

        [JsonPropertyName("descripcion")]
        public string? Descripcion { get; set; }

        [JsonPropertyName("tipoDescuento")]
        public string TipoDescuento { get; set; }

        [JsonPropertyName("codigoCupon")]
        public string? CodigoCupon { get; set; }

        [JsonPropertyName("valor")]
        public decimal Valor { get; set; }

        [JsonPropertyName("fechaInicio")]
        public DateTime? FechaInicio { get; set; }

        [JsonPropertyName("fechaFin")]
        public DateTime? FechaFin { get; set; }

        [JsonPropertyName("limiteUsosTotal")]
        public int? LimiteUsosTotal { get; set; }

        [JsonPropertyName("usosActuales")]
        public int UsosActuales { get; set; }

        [JsonPropertyName("estaActivo")]
        public bool EstaActivo { get; set; }
    }
}