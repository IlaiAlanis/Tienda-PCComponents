using System.Text.Json.Serialization;

namespace API_TI.Models.DTOs.NotificacionDTOs
{
    public class NotificacionDto
    {
        [JsonPropertyName("idNotificacion")]
        public int IdNotificacion { get; set; }

        [JsonPropertyName("titulo")]
        public string Titulo { get; set; } = string.Empty;

        [JsonPropertyName("mensaje")]
        public string Mensaje { get; set; } = string.Empty;

        [JsonPropertyName("tipo")]
        public string Tipo { get; set; } = string.Empty;

        [JsonPropertyName("leida")]
        public bool Leido { get; set; }

        [JsonPropertyName("fechaCreacion")]
        public DateTime FechaCreacion { get; set; }
    }
}
