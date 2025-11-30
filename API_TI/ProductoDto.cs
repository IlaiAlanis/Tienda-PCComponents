using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.ProductoDTOs
{
    public class ProductoDto
    {
        [JsonPropertyName("idProducto")]
        public int IdProducto { get; set; }

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = null!;

        [JsonPropertyName("descripcion")]
        public string? Descripcion { get; set; }

        [JsonPropertyName("dimensiones")]
        public string? Dimensiones { get; set; }

        [JsonPropertyName("peso")]
        public decimal Peso { get; set; }

        [JsonPropertyName("esDestacado")]
        public bool EsDestacado { get; set; }

        [JsonPropertyName("precioBase")]
        public decimal PrecioBase { get; set; }

        [JsonPropertyName("precioPromocional")]
        public decimal? PrecioPromocional { get; set; }

        [JsonPropertyName("sku")]
        public string Sku { get; set; } = string.Empty;

        [JsonPropertyName("codigoBarras")]
        public string CodigoBarras { get; set; } = string.Empty;

        [JsonPropertyName("stock")]
        public int? Stock { get; set; }

        [JsonPropertyName("estaActivo")]
        public bool EstaActivo { get; set; }

        [JsonPropertyName("fechaCreacion")]
        public DateTime FechaCreacion { get; set; }

        // Relationships
        [JsonPropertyName("categoria")]
        public string Categoria { get; set; } = string.Empty;

        [JsonPropertyName("marca")]
        public string Marca { get; set; } = string.Empty;

        [JsonPropertyName("proveedor")]
        public string Proveedor { get; set; } = string.Empty;
        [JsonPropertyName("imagenUrl")]
        public string? ImagenUrl => Imagenes?.FirstOrDefault()?.UrlImagen;

        [JsonPropertyName("imagenes")]
        public List<ProductoImagenDto>? Imagenes { get; set; } = new();

        [JsonPropertyName("variaciones")]
        public List<ProductoVariacionDto> Variaciones { get; set; } = new();

        [JsonPropertyName("resenas")]
        public List<ProductoResenaDto> Resenas { get; set; } = new();
    }
}
