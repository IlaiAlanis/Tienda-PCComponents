using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.ProveedorDTOs
{
    public class CreateProveedorRequest
    {
        [JsonPropertyName("paisId")]
        public int PaisId { get; set; }

        [JsonPropertyName("estadoId")]
        public int EstadoId { get; set; }

        [JsonPropertyName("ciudadId")]
        public int CiudadId { get; set; }

        [JsonPropertyName("codigoPostal")]
        public string? CodigoPostal { get; set; }

        [JsonPropertyName("nombreProveedor")]
        [Required(ErrorMessage = "El nombre del proveedor es requerido")]
        public string NombreProveedor { get; set; }

        [JsonPropertyName("contacto")]
        [StringLength(100, ErrorMessage = "El nombre de contacto no puede exceder 100 caracteres")]
        public string? NombreContacto { get; set; }

        [JsonPropertyName("telefono")]
        [Required(ErrorMessage = "El teléfono es requerido")]
        public string Telefono { get; set; }

        [JsonPropertyName("email")]
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Correo { get; set; }

        [JsonPropertyName("direccion")]
        [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        public string? Direccion { get; set; }

        [JsonPropertyName("estaActivo")]
        public bool? EstaActivo { get; set; }
    }
}
