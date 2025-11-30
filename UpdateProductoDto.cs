using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.ProductoDTOs
{
    public class UpdateProductoDto
    {
        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }
        [JsonPropertyName("descripcion")]
        public string? Descripcion { get; set; }
        [JsonPropertyName("dimensiones")]
        public string? Dimensiones { get; set; }
        [JsonPropertyName("peso")]
        public decimal? Peso { get; set; }
        [JsonPropertyName("precioBase")]
        public decimal? PrecioBase { get; set; }
        [JsonPropertyName("precioPromocional")]
        public decimal? PrecioPromocional { get; set; }
        [JsonPropertyName("esDestacado")]
        public bool? EsDestacado { get; set; }
        [JsonPropertyName("estaActivo")]
        public bool? EstaActivo { get; set; }
        [JsonPropertyName("stock")]
        public int? Stock { get; set; }
        [JsonPropertyName("categoriaId")]
        public int? CategoriaId { get; set; }

        [JsonPropertyName("marcaId")]
        public int? MarcaId { get; set; }

        [JsonPropertyName("proveedorId")]
        public int? ProveedorId { get; set; }

        [JsonPropertyName("sku")]
        public string? Sku { get; set; }

        [JsonPropertyName("codigoBarras")]
        public string? CodigoBarras { get; set; }
    }
}
