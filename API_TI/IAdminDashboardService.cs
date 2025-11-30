using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.AdminDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<ApiResponse<DashboardMetricsDto>> GetMetricsAsync();
    }
}
