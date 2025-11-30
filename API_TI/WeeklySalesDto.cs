using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.AdminDTOs
{
    public class WeeklySalesDto
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }
        [JsonPropertyName("value")]
        public decimal Value { get; set; }
    }
}
