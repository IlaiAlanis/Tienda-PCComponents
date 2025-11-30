using System.ComponentModel.DataAnnotations;

namespace API_TI.Models.DTOs.FaqDTOs
{
    public class CreateFaqRequest
    {
        [Required(ErrorMessage = "La pregunta es requerida")]
        [StringLength(500)]
        public string Pregunta { get; set; }

        [Required(ErrorMessage = "La respuesta es requerida")]
        [StringLength(2000)]
        public string Respuesta { get; set; }

        [Required(ErrorMessage = "La categoría es requerida")]
        [StringLength(50)]
        public string Categoria { get; set; } // Pedidos, Envíos, Devoluciones, Productos, Cuenta

        public int? Orden { get; set; }

        public bool EstaActivo { get; set; } = true;
    }
}
