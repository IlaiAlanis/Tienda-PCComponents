namespace API_TI.Models.DTOs.UsuarioDTOs
{
    public class UsuarioTokenDto
    {
        public int IdToken { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaExpiracion { get; set; }
        public bool Revoked { get; set; }
        public bool Usado { get; set; }
        public string? DeviceName { get; set; }
        public string? UserAgentShort { get; set; }
        public string? CreatedByIp { get; set; }
        public bool IsCurrent { get; set; }
    }
}
