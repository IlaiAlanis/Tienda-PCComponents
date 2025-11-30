using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API_TI.Models.Auth
{
    public class RegisterRequestAuth
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 150 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9_\s]+$", ErrorMessage = "El nombre solo puede contener letras, números y guiones bajos")]
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }  
        
        [Required(ErrorMessage = "El nombre de usuario es requerido")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 150 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9_\s]+$", ErrorMessage = "El nombre solo puede contener letras, números y guiones bajos")]
        [JsonPropertyName("nombreUsuario")]
        public string NombreUsuario { get; set; }

        [JsonPropertyName("apellidoPaterno")]
        public string? ApellidoPaterno { get; set; }

        [JsonPropertyName("apellidoMaterno")]
        public string? ApellidoMaterno { get; set; }

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        [StringLength(255)]
        public string Correo { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#._\-+(){}[\]=/\\|~^,;:])[A-Za-z\d@$!%*?&#._\-+(){}[\]=/\\|~^,;:]{8,}$",
            ErrorMessage = "La contraseña debe contener mayúscula, minúscula, número y un carácter especial permitido.")]
        public string Contrasena { get; set; }

        [Required]
        [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarContrasena { get; set; }

        public DateOnly? FechaNacimiento { get; set; }

        public int RolId { get; set; } = 2;

        public string? RecaptchaToken { get; set; }
    }
}
