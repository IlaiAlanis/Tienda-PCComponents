using API_TI.Models.dbModels;
using System.Security.Claims;

namespace API_TI.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(Usuario user, string role);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
