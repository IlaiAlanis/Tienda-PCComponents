using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.CategoriaDTOs
{
    public class UpdateCategoriaDto
    {
        [JsonPropertyName("idCategoria")]
        public int IdCategoria { get; set; }

        [JsonPropertyName("nombre")]
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Nombre { get; set; }

        [JsonPropertyName("descripcion")]
        public string? Descripcion { get; set; }

        [JsonPropertyName("estaActivo")]
        public bool EstaActivo { get; set; }
    }
}
