using System.ComponentModel.DataAnnotations;

namespace API_TI.Models.Auth
{
    public class VerifyEmailRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Code { get; set; }
    }
}
