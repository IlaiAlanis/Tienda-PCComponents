using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.UsuarioDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IUserProfileService
    {
        Task<ApiResponse<UserProfileDto>> GetProfileAsync(int usuarioId);
        Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(int usuarioId, UpdateProfileRequest request);
        Task<ApiResponse<object>> ChangePasswordAsync(int usuarioId, ChangePasswordRequest request);
    }
}
