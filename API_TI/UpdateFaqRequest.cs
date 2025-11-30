using System.ComponentModel.DataAnnotations;

namespace API_TI.Models.DTOs.FaqDTOs
{
    public class UpdateFaqRequest
    {
        [Required]
        public string Pregunta { get; set; }

        [Required]
        public string Respuesta { get; set; }

        [Required]
        public string Categoria { get; set; }

        public int? Orden { get; set; }

        public bool EstaActivo { get; set; }
    }
}
