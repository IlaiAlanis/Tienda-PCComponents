using System.ComponentModel.DataAnnotations;

namespace API_TI.Models.DTOs.ContactoDTOs
{
    public class ContactRequest
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El motivo es requerido")]
        public string Motivo { get; set; }

        [Required(ErrorMessage = "El mensaje es requerido")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "El mensaje debe tener entre 10 y 1000 caracteres")]
        public string Mensaje { get; set; }
    }
}
