using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.CategoriaDTOs
{
    public class CategoriaDto
    {
        [JsonPropertyName("idCategoria")]
        public int IdCategoria { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = null!;

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; } = null!;

        [JsonPropertyName("estaActivo")]
        public bool EstaActivo { get; set; }
    }
}
