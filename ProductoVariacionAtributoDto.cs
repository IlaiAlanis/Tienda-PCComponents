using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.ProductoDTOs
{
    public class ProductoVariacionAtributoDto
    {
        [JsonPropertyName("idAtributo")]
        public int AtributoId { get; set; }
        [JsonPropertyName("nombre")]
        public string NombreAtributo { get; set; } = string.Empty;
        [JsonPropertyName("valor")]
        public string Valor { get; set; } = string.Empty;
    }
}
