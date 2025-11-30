using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.ProductoDTOs
{
    public class ProductoVariacionDto
    {
        [JsonPropertyName("idVariacion")]
        public int IdVariacion { get; set; }
        [JsonPropertyName("productoId")]
        public int ProductoId { get; set; }
        [JsonPropertyName("sku")]
        public string? Sku { get; set; }
        [JsonPropertyName("codigoBarras")]
        public string CodigoBarras { get; set; } = string.Empty;
        [JsonPropertyName("precio")]
        public decimal Precio { get; set; }
        [JsonPropertyName("imagenUrl")]
        public string? ImagenUrl { get; set; }
        [JsonPropertyName("stock")]
        public int Stock { get; set; }
        [JsonPropertyName("estaActivo")]
        public bool EstaActivo { get; set; }


        // Relationship
        [JsonPropertyName("categoria")]
        public string Categoria { get; set; } = string.Empty;
        [JsonPropertyName("marca")]
        public string Marca { get; set; } = string.Empty;
        [JsonPropertyName("proveedor")]
        public string Proveedor { get; set; } = string.Empty;
        [JsonPropertyName("atributos")]
        public List<ProductoVariacionAtributoDto> Atributos { get; set; } = new();
    }
}
