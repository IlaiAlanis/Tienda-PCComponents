using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.DireccionDTOs
{
    public class DireccionDto
    {
        [JsonPropertyName("idDireccion")]
        public int IdDireccion { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("telefono")]
        public string? Telefono { get; set; }

        [JsonPropertyName("usuarioId")]
        public int UsuarioId { get; set; }

        [JsonPropertyName("paisId")]
        public int? PaisId { get; set; }

        [JsonPropertyName("estadoId")]
        public int? EstadoId { get; set; }

        [JsonPropertyName("ciudadId")]
        public int? CiudadId { get; set; }

        [JsonPropertyName("pais")]
        public string? PaisNombre { get; set; }

        [JsonPropertyName("estado")]
        public string? EstadoNombre { get; set; }

        [JsonPropertyName("ciudad")]
        public string? CiudadNombre { get; set; }

        [JsonPropertyName("codigoPostal")]
        public string CodigoPostal { get; set; } = string.Empty;

        [JsonPropertyName("colonia")]
        public string? Colonia { get; set; }

        [JsonPropertyName("calle")]
        public string Calle { get; set; }

        [JsonPropertyName("numeroExterior")]
        public string? NumeroExterior { get; set; }

        [JsonPropertyName("numeroInterior")]
        public string? NumeroInterior { get; set; }

        [JsonPropertyName("etiqueta")]
        public string Etiqueta { get; set; }

        [JsonPropertyName("direccionCompleta")]
        public string? DireccionCompleta { get; set; }

        [JsonPropertyName("esPrincipal")]
        public bool EsDefault { get; set; }

        [JsonPropertyName("referencia")]
        public string Referencia { get; set; }

        [JsonPropertyName("latitud")]
        public decimal? Latitud { get; set; }

        [JsonPropertyName("longitud")]
        public decimal? Longitud { get; set; }

        [JsonPropertyName("googlePlaceId")]
        public string? GooglePlaceId { get; set; }

        [JsonPropertyName("notas")]
        public string? Notas { get; set; }

        [JsonPropertyName("esPrincipal")]
        public bool EsPrincipal { get; set; }

        [JsonPropertyName("fechaCreacion")]
        public DateTime FechaCreacion { get; set; }

        [JsonPropertyName("fechaActualizacion")]
        public DateTime? FechaActualizacion { get; set; }

        [JsonPropertyName("tieneCoordinadas")]
        public bool TieneCoordinadas => Latitud.HasValue && Longitud.HasValue;

        [JsonPropertyName("direccionFormateada")]
        public string DireccionFormateada
        {
            get
            {
                var parts = new List<string>();

                if (!string.IsNullOrWhiteSpace(Calle)) parts.Add(Calle);
                if (!string.IsNullOrWhiteSpace(NumeroExterior)) parts.Add($"#{NumeroExterior}");
                if (!string.IsNullOrWhiteSpace(Colonia)) parts.Add(Colonia);
                parts.Add($"{CiudadNombre}, {EstadoNombre} {CodigoPostal}");

                return string.Join(", ", parts);
            }
        }
    }
}
