using System.ComponentModel.DataAnnotations;

namespace API_TI.Models.DTOs.NewsletterDTOs
{
    public class SubscribeRequest
    {
        [Required(ErrorMessage = "Email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }
    }
}
