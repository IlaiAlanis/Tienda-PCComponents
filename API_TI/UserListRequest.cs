using CloudinaryDotNet.Actions;
using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.AdminDTOs
{
    public class UserListRequest
    {

        [JsonPropertyName("page")]
        public int Page { get; set; } = 1;

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; } = 10;

        [JsonPropertyName("search")]
        public string? Search { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonIgnore]
        public bool? EstaActivo
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Status))
                    return null;

                return Status.ToLower() switch
                {
                    "active" => true,
                    "inactive" => false,
                    "true" => true,
                    "false" => false,
                    _ => null
                };
            }
        }

        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonIgnore]
        public int? RolId
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Role))
                    return null;

                // Map role names to IDs
                // Adjust these IDs based on your database
                return Role.ToUpper() switch
                {
                    "Admin" => 1,
                    "USER" => 2,
                    _ => null
                };
            }
        }
    }
}
