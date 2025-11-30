using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.UsuarioDTOs
{
    public class DeleteAccountRequest
    {
        [JsonPropertyName("password")]
        [Required(ErrorMessage = "La contraseña es requerida para eliminar la cuenta")]
        public string Password { get; set; } = string.Empty;
    }
}
