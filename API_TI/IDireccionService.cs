using API_TI.Models.ApiResponse;
using API_TI.Models.DTOs.DireccionDTOs;

namespace API_TI.Services.Interfaces
{
    public interface IDireccionService
    {
        Task<ApiResponse<IList<DireccionDto>>> GetUserAddressesAsync(int userId);
        Task<ApiResponse<DireccionDto>> GetAddressByIdAsync(int userId, int addressId);
        Task<ApiResponse<DireccionDto>> CreateAddressAsync(int userId, CreateDireccionRequest request);
        Task<ApiResponse<DireccionDto>> UpdateAddressAsync(int userId, int addressId, UpdateDireccionRequest request);
        Task<ApiResponse<object>> DeleteAddressAsync(int userId, int addressId);
        Task<ApiResponse<DireccionDto>> SetDefaultAddressAsync(int userId, int addressId);
    }
}
