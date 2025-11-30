using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.CategoriaDTOs
{
    public class CreateCategoriaDto
    {
        [JsonPropertyName("nombre")]
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Nombre { get; set; } = null!;

        [JsonPropertyName("descripcion")]
        public string? Descripcion { get; set; }
    }
}
