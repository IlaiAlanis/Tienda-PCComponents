using System.ComponentModel.DataAnnotations;

namespace API_TI.Models.DTOs.ReviewDTOs
{
    public class CreateReviewRequest
    {
        [Required]
        public int ProductoId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Calificación debe ser entre 1 y 5")]
        public int Calificacion { get; set; }

        [MaxLength(1000)]
        public string? Comentario { get; set; }
    }
}
