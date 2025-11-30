using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.AdminDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IAdminService
    {
        Task<ApiResponse<PagedResult<AdminUserDto>>> GetUsersAsync(UserListRequest request);
        Task<ApiResponse<AdminUserDto>> GetUserDetailsAsync(int userId);
        Task<ApiResponse<object>> ToggleUserStatusAsync(int adminId, int userId);
        Task<ApiResponse<AdminDashboardDto>> GetDashboardStatsAsync();
    }
}
