using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.AdminDTOs;
using API_TI.Models.DTOs.DescuentoDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IDescuentoAdminService
    {
        Task<ApiResponse<PagedResult<DescuentoDto>>> GetAllDiscountsAsync(
            string? search = null,
            string? status = null,
            int page = 1,
            int pageSize = 10); 
        Task<ApiResponse<DescuentoDetailDto>> GetDiscountByIdAsync(int id);
        Task<ApiResponse<DescuentoDto>> CreateDiscountAsync(CreateDescuentoRequest request, int adminId);
        Task<ApiResponse<DescuentoDto>> UpdateDiscountAsync(int id, UpdateDescuentoRequest request, int adminId);
        Task<ApiResponse<object>> DeleteDiscountAsync(int id, int adminId);
        Task<ApiResponse<object>> ValidateCouponAsync(string codigo, int usuarioId);
    }
}
