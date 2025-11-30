using API_TI.Models.DTOs.UsuarioDTOs;

namespace API_TI.Models.Auth
{
    public class AuthResponse
    {
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime ExpiraEn { get; set; } 
        public UsuarioDto Usuario { get; set; } = null!;
      
    }
}
