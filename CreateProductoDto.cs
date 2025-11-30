using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.ProductoDTOs
{
    public class CreateProductoDto
    {
        [JsonPropertyName("categoriaId")]
        [Required(ErrorMessage = "La categoría es requerida")]
        public int CategoriaId { get; set; }
        [JsonPropertyName("proveedorId")]
        public int ProveedorId { get; set; }
        [JsonPropertyName("marcaId")]
        [Required(ErrorMessage = "La marca es requerida")]
        public int MarcaId { get; set; }
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;
        [JsonPropertyName("descripcion")]
        public string? Descripcion { get; set; }
        [JsonPropertyName("dimensiones")]
        public string? Dimensiones { get; set; }
        [JsonPropertyName("precioBase")]
        [Required(ErrorMessage = "El precio base es requerido")]
        public decimal? Peso { get; set; }
        [JsonPropertyName("precioBase")]
        public decimal PrecioBase { get; set; }
        [JsonPropertyName("precioPromocional")]
        public decimal? PrecioPromocional { get; set; }
        [JsonPropertyName("esDestacado")]
        public bool EsDestacado { get; set; } = false;
        [JsonPropertyName("sku")]
        [Required(ErrorMessage = "El SKU es requerido")]
        public string Sku { get; set; } = string.Empty;
        [JsonPropertyName("codigoBarras")]
        public string CodigoBarras { get; set; } = string.Empty;
        [JsonPropertyName("stock")]
        public int Stock { get; set; } = 0;
        [JsonPropertyName("estaActivo")]
        public bool EstaActivo { get; set; } = true;
    }
}
