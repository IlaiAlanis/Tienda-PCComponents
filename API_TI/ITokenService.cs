using API_TI.Models.dbModels;
using API_TI.Models.DTOs.UsuarioDTOs;

namespace API_TI.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateRefreshTokenPlain();
        string HashToken(string tokenPlain);
        Task<string> CreateAndStoreRefreshTokenAsync(Usuario user, HttpContext? ctx = null);
        Task<UsuarioToken?> ValidateRefreshTokenByHashAsync(string refreshTokenPlain);
        Task<string> RotateRefreshTokenAsync(Usuario user, UsuarioToken existingToken, HttpContext? ctx = null);
        Task RevokeRefreshTokenAsync(string refreshTokenPlain, string? reason = null);
        Task<IList<UsuarioTokenDto>> ListActiveSessionsAsync(int userId, string? currentRefreshPlain = null);
        Task<bool> RevokeSessionAsync(int userId, int tokenId);
        Task RevokeAllUserTokensAsync(int userId, string reason = "Security");

    }
}
