using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.ProductoDTOs
{
    public class ProductoVariacionCreateDto
    {
        [JsonPropertyName("productoId")]
        public int ProductoId { get; set; }

        [JsonPropertyName("sku")]
        public string? Sku { get; set; }

        [JsonPropertyName("codigoBarras")]
        public string? CodigoBarras { get; set; }

        [JsonPropertyName("imagenUrl")]
        public string? ImagenUrl { get; set; }

        [JsonPropertyName("precio")]
        public decimal Precio { get; set; }

        [JsonPropertyName("stock")]
        public int Stock { get; set; }

        [JsonPropertyName("estaActivo")]
        public bool EstaActivo { get; set; } = true;

        [JsonPropertyName("atributos")]
        public List<VariacionAtributoCreateDto>? Atributos { get; set; }
    }
}
