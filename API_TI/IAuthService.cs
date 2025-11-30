using API_TI.Models.ApiResponse;
using API_TI.Models.Auth;
using API_TI.Models.dbModels;

namespace API_TI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequestAuth request);
        Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequestAuth request);
        Task<ApiResponse<AuthResponse>> LoginWithGoogleAsync(GoogleAuthRequestAuth request);
        Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string refreshTokenPlain);
        Task<ApiResponse<object>> RequestPasswordResetAsync(string email);
        Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordRequestAuth request);
        Task<ApiResponse<object>> SendEmailVerificationAsync(int userId);
        Task<ApiResponse<object>> VerifyEmailAsync(VerifyEmailRequest request);
        Task<ApiResponse<object>> LogoutAsync(string refreshTokenPlain);
    }
}
