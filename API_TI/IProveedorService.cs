using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.ProveedorDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IProveedorService
    {
        Task<ApiResponse<List<ProveedorDto>>> GetAllAsync();
        Task<ApiResponse<ProveedorDto>> GetByIdAsync(int id);
        Task<ApiResponse<ProveedorDto>> CreateAsync(CreateProveedorRequest request);
        Task<ApiResponse<ProveedorDto>> UpdateAsync(int id, CreateProveedorRequest request);
        Task<ApiResponse<object>> DeleteAsync(int id);
    }
}
