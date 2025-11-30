using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.ProductoDTOs
{
    public class ProductoSearchRequest
    {
        [JsonPropertyName("searchTerm")]
        public string? Query { get; set; }

        [JsonPropertyName("categoriaIds")]
        public int[]? CategoriaIds { get; set; }

        [JsonPropertyName("categoriaId")]
        public int? CategoriaId { get; set; }

        [JsonPropertyName("marcaIds")]
        public int[]? MarcaIds { get; set; }

        [JsonPropertyName("marcaId")]
        public int? MarcaId { get; set; }

        [JsonPropertyName("minPrice")]
        public decimal? PrecioMin { get; set; }

        [JsonPropertyName("maxPrice")]
        public decimal? PrecioMax { get; set; }

        [JsonPropertyName("enStock")]  
        public bool? EnStock { get; set; }    
        
        [JsonPropertyName("stock")]
        public int? Stock { get; set; }

        [JsonPropertyName("enDescuento")]
        public bool? EnDescuento { get; set; }  

        [JsonPropertyName("orderBy")]
        public string OrderBy { get; set; } = "nombre";

        [JsonPropertyName("orderDirection")]  
        public string OrderDirection { get; set; } = "asc";

        [JsonPropertyName("page")]
        public int Page { get; set; } = 1;

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; } = 20;
    }
}