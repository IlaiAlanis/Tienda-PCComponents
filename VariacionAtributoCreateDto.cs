using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.ProductoDTOs
{
    public class VariacionAtributoCreateDto
    {
        [JsonPropertyName("atributoId")]
        public int AtributoId { get; set; }

        [JsonPropertyName("valor")]
        public string Valor { get; set; } = string.Empty;
    }
}
