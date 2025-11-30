using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.ConfGlobalDTOs
{
    public class PreferencesDto
    {
        [JsonPropertyName("theme")]
        public string Theme { get; set; } = "dark";  // 'light', 'dark', 'auto'

        [JsonPropertyName("language")]
        public string Language { get; set; } = "es";  // 'es', 'en', 'fr'

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "USD";  // 'USD', 'EUR', 'MXN'

        [JsonPropertyName("emailNotifications")]
        public bool EmailNotifications { get; set; } = true;

        [JsonPropertyName("promoNotifications")]
        public bool PromoNotifications { get; set; } = false;

        [JsonPropertyName("stockNotifications")]
        public bool StockNotifications { get; set; } = true;

        [JsonPropertyName("publicProfile")]
        public bool PublicProfile { get; set; } = false;

        [JsonPropertyName("shareData")]
        public bool ShareData { get; set; } = false;
    }
}
