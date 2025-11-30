using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.UsuarioDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<UserProfileDto>> GetProfileAsync(int userId);
        Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(int userId, UpdateProfileRequest request);
        Task<ApiResponse<object>> ChangePasswordAsync(int userId, ChangePasswordRequest request);
        Task<ApiResponse<object>> UpdateEmailAsync(int userId, UpdateEmailRequest request);
        Task<ApiResponse<object>> DeleteAccountAsync(int userId, string password);
    }
}
