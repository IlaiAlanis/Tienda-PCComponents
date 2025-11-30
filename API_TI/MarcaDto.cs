using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.MarcaDTOs
{
    public class MarcaDto
    {
        [JsonPropertyName("idMarca")]
        public int IdMarca { get; set; }
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = null!;
        [JsonPropertyName("descripcion")]
        public string? Descripcion { get; set; }
        [JsonPropertyName("estaActivo")]

        public bool EstaActivo { get; set; }
    }
}
