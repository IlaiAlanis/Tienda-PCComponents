using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.ConfGlobalDTOs
{
    public class UpdatePreferencesRequest
    {
        [JsonPropertyName("theme")]
        public string? Theme { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("emailNotifications")]
        public bool? EmailNotifications { get; set; }

        [JsonPropertyName("promoNotifications")]
        public bool? PromoNotifications { get; set; }

        [JsonPropertyName("stockNotifications")]
        public bool? StockNotifications { get; set; }

        [JsonPropertyName("publicProfile")]
        public bool? PublicProfile { get; set; }

        [JsonPropertyName("shareData")]
        public bool? ShareData { get; set; }
    }
}
