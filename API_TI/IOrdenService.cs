using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.AdminDTOs;
using API_TI.Models.DTOs.OrdenDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IOrdenService
    {
        // User methods
        Task<ApiResponse<PagedResult<OrdenDto>>> GetUserOrdersAsync(int usuarioId, int page, int pageSize);
        Task<ApiResponse<OrdenDto>> GetOrderByIdAsync(int usuarioId, int ordenId);
        Task<ApiResponse<object>> CancelOrderAsync(int ordenId, int usuarioId);

        // Admin methods
        Task<ApiResponse<PagedResult<OrdenDto>>> GetAllOrdersAsync(
            string? search = null,
            string? status = null,
            int page = 1,
            int pageSize = 10);
        Task<ApiResponse<OrdenDto>> GetOrderByIdAdminAsync(int ordenId);
        Task<ApiResponse<object>> UpdateOrderStatusAsync(int ordenId, string nuevoEstado, int adminId);
    }
}
