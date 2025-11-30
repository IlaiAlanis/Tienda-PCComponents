using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.WishlistDTOs
{
    public class WishlistDto
    {
        [JsonPropertyName("idListaDeseo")]
        public int IdListaDeseo { get; set; }

        [JsonPropertyName("usuarioId")]
        public int UsuarioId { get; set; }

        [JsonPropertyName("items")]
        public List<WishlistItemDto> Items { get; set; }

        [JsonPropertyName("totalItems")]
        public int TotalItems { get; set; }
    }
}
