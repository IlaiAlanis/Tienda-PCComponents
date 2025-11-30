using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.ProductoDTOs
{
    public class ProductoSimpleDto
    {
        [JsonPropertyName("imagenes")]
        public List<string>? Imagenes { get; set; }
    }
}
