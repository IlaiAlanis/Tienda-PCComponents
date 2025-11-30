using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.DireccionDTOs
{
    public class CreateDireccionRequest
    {
        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }

        [JsonPropertyName("paisId")]
        public int? PaisId { get; set; }

        [JsonPropertyName("estadoId")] 
        public int? EstadoId { get; set; }

        [JsonPropertyName("ciudadId")]
        public int? CiudadId { get; set; }

        [JsonPropertyName("telefono")]
        [Phone(ErrorMessage = "Teléfono inválido")]
        [MaxLength(20)]
        public string? Telefono { get; set; }

        [JsonPropertyName("calle")]
        [Required(ErrorMessage = "La calle es requerida")]
        public string Calle { get; set; } = string.Empty;

        [JsonPropertyName("ciudad")]
        public string? CiudadNombre { get; set; }

        [JsonPropertyName("estado")]
        public string? EstadoNombre { get; set; }
        
        [JsonPropertyName("pais")]
        public string? PaisNombre { get; set; }

        [JsonPropertyName("numeroExterior")]
        public string? NumeroExterior { get; set; }

        [JsonPropertyName("numeroInterior")]
        public string? NumeroInterior { get; set; }

        [JsonPropertyName("codigoPostal")]
        [Required(ErrorMessage = "El código postal es requerido")]
        public string CodigoPostal { get; set; }

        [JsonPropertyName("colonia")]
        [MaxLength(100, ErrorMessage = "La colonia no puede exceder 100 caracteres")]
        public string Colonia { get; set; }

        [JsonPropertyName("esPrincipal")]
        public bool EsDefault { get; set; }

        [JsonPropertyName("referencia")]
        public string? Referencia { get; set; }

        // Google Places
        [JsonPropertyName("googlePlaceId")]
        public string PlaceId { get; set; }

        [JsonPropertyName("latitud")]
        [Range(-90, 90, ErrorMessage = "Latitud debe estar entre -90 y 90")]
        public decimal? Latitud { get; set; }

        [JsonPropertyName("longitud")]
        [Range(-180, 180, ErrorMessage = "Longitud debe estar entre -180 y 180")]
        public decimal? Longitud { get; set; }

        [JsonPropertyName("direccionCompleta")]
        [MaxLength(500)]
        public string? DireccionCompleta { get; set; }

        [JsonIgnore]
        public string Etiqueta => Nombre ?? "Principal";

        [JsonPropertyName("notas")]
        [MaxLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
        public string? Notas { get; set; }

        [JsonIgnore]
        public bool HasValidLocation =>
            (PaisId.HasValue && EstadoId.HasValue && CiudadId.HasValue) ||
            (!string.IsNullOrWhiteSpace(CiudadNombre) && !string.IsNullOrWhiteSpace(EstadoNombre));
    }
}
